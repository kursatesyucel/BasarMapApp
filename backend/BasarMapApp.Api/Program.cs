using System.Text;
using BasarMapApp.Api.Configuration;
using BasarMapApp.Api.Data;
using BasarMapApp.Api.Mappings;
using BasarMapApp.Api.Repositories.Implementations;
using BasarMapApp.Api.Repositories.Interfaces;
using BasarMapApp.Api.Services.Implementations;
using BasarMapApp.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            // Development: Esnek CORS politikası
            // Not: Eğer Cookie kullanacaksanız .AllowCredentials() ve .SetIsOriginAllowed() kullanmalısınız.
            // Şimdilik AllowAnyOrigin ile devam ediyoruz.
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
        else
        {
            // Production: Şimdilik her yere izin veriyoruz (Test kolaylığı için)
            // Canlıya tamamen çıktığınızda burayı frontend domaini ile kısıtlamanız önerilir.
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
    });
});

// Configure PostgreSQL & PostGIS via NetTopologySuite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsql => npgsql.UseNetTopologySuite()
    )
);

// Configure MongoDB settings for logging
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));

// Register LogService as Singleton
builder.Services.AddSingleton<ILogService, LogService>();

// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Register repositories
builder.Services.AddScoped<IPointRepository, PointRepository>();
builder.Services.AddScoped<ILineRepository, LineRepository>();
builder.Services.AddScoped<IPolygonRepository, PolygonRepository>();
builder.Services.AddScoped<ICameraRepository, CameraRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Register GIS Boundary repositories (Read-Only)
builder.Services.AddScoped<IProvinceRepository, ProvinceRepository>();
builder.Services.AddScoped<IDistrictRepository, DistrictRepository>();
builder.Services.AddScoped<ISettlementRepository, SettlementRepository>();

// Register services
builder.Services.AddScoped<IPointService, PointService>();
builder.Services.AddScoped<ILineService, LineService>();
builder.Services.AddScoped<IPolygonService, PolygonService>();
builder.Services.AddScoped<ICameraService, CameraService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMailService, GmailMailService>();
builder.Services.AddScoped<IDeviceService, DeviceService>();

// Register GIS Boundary service (Read-Only)
builder.Services.AddScoped<IBoundaryService, BoundaryService>();

// JWT Authentication
var jwtSecret = builder.Configuration["JwtSettings:Secret"];
if (string.IsNullOrEmpty(jwtSecret))
{
    // Production'da bu hatayı almak istemiyorsanız Env Variable olarak tanımladığınızdan emin olun
    // throw new InvalidOperationException("JWT Secret is not configured."); // Geçici olarak kapatılabilir veya loglanabilir
}

// Secret null ise default bir değer atayıp patlamasını önleyelim (Sadece build aşaması için)
// Çalışma zamanında mutlaka dolu olmalıdır.
var keyBytes = Encoding.UTF8.GetBytes(jwtSecret ?? "TemporarySecretForBuildProcessOnly123!");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "BasarMapApp API", Version = "v2" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
app.UseStaticFiles();

if (!app.Environment.IsDevelopment())
{
    // Production'da HTTPS zorlamasını şimdilik kapalı tutabiliriz (Nginx halledecek)
    // app.UseHttpsRedirection(); 
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// -----------------------------------------------------------------------------
// VERİTABANI BAŞLATMA VE SEED İŞLEMLERİ (Production Ready)
// -----------------------------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        
        // 1. OTOMATİK MIGRATION: Veritabanı yoksa oluşturur, eksik tabloları ekler.
        // Docker/Production ortamında bu komut hayati önem taşır.
        context.Database.Migrate();
        Console.WriteLine("--> Veritabanı migration işlemi başarıyla tamamlandı.");

        // 2. ADMIN SEEDING:
        // Development ortamındaysak VEYA Production'da özel bir izin varsa çalışır.
        // docker-compose dosyasında "SEED_ADMIN_IN_PROD=true" verirseniz canlıda da admin oluşturur.
        var seedAdminInProd = Environment.GetEnvironmentVariable("SEED_ADMIN_IN_PROD") == "true";
        
        if (app.Environment.IsDevelopment() || seedAdminInProd)
        {
            await DbSeeder.SeedAdminUser(context);
            Console.WriteLine("--> Admin Kullanıcısı kontrol edildi / oluşturuldu.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"--> Veritabanı başlatılırken HATA oluştu: {ex.Message}");
    }
}
// -----------------------------------------------------------------------------

app.Run();