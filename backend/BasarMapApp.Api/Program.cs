using BasarMapApp.Api.Data;
using BasarMapApp.Api.Mappings;
using BasarMapApp.Api.Repositories.Implementations;
using BasarMapApp.Api.Repositories.Interfaces;
using BasarMapApp.Api.Services.Implementations;
using BasarMapApp.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure PostgreSQL & PostGIS via NetTopologySuite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsql => npgsql.UseNetTopologySuite()
    )
);

// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Register repositories
builder.Services.AddScoped<IPointRepository, PointRepository>();
builder.Services.AddScoped<ILineRepository, LineRepository>();
builder.Services.AddScoped<IPolygonRepository, PolygonRepository>();

// Register services
builder.Services.AddScoped<IPointService, PointService>();
builder.Services.AddScoped<ILineService, LineService>();
builder.Services.AddScoped<IPolygonService, PolygonService>();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
