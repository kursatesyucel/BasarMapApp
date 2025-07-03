# 🗺️ BasarMapApp Backend API

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-14+-336791?style=flat&logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![PostGIS](https://img.shields.io/badge/PostGIS-3.0+-FF6600?style=flat&logo=postgis&logoColor=white)](https://postgis.net/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

Modern harita uygulaması için geliştirilmiş **RESTful Web API**. Coğrafi veri yönetimi, geometrik nesne işlemleri ve gerçek zamanlı harita güncellemeleri sağlar.

## 🚀 Özellikler

### 🎯 **Geometrik Veri Yönetimi**
- **Points**: Nokta bazlı konum verisi
- **Lines**: Çizgi/rota geometrisi  
- **Polygons**: Alan/bölge geometrisi
- **PostGIS** entegrasyonu ile gelişmiş coğrafi işlemler

### 🏗️ **Mimari & Teknolojiler**
- **.NET 8.0** Web API
- **Clean Architecture** (Repository + Service Pattern)
- **Entity Framework Core** ORM
- **PostgreSQL + PostGIS** veritabanı
- **NetTopologySuite** geometrik işlemler
- **AutoMapper** object mapping
- **Swagger/OpenAPI** dokümantasyon

### 🔧 **API Özellikleri**
- **CRUD işlemleri** tüm geometrik nesneler için
- **Validation** ve error handling
- **CORS** frontend entegrasyonu
- **Strongly-typed DTOs**
- **Async/await** pattern

## 📁 Proje Yapısı

```
BasarMapApp.Api/
├── Controllers/          # API endpoint controllers
│   ├── PointsController.cs
│   ├── LinesController.cs
│   └── PolygonsController.cs
├── Data/                 # Entity Framework context
│   └── ApplicationDbContext.cs
├── DTOs/                 # Data Transfer Objects
│   ├── Point/
│   ├── Line/
│   └── Polygon/
├── Models/               # Entity models
│   ├── MapPoint.cs
│   ├── Line.cs
│   └── Polygon.cs
├── Repositories/         # Data access layer
│   ├── Interfaces/
│   └── Implementations/
├── Services/            # Business logic layer
│   ├── Interfaces/
│   └── Implementations/
├── Mappings/           # AutoMapper profiles
│   └── MappingProfile.cs
├── Responses/          # API response models
│   └── ApiResponse.cs
└── Migrations/         # EF Core migrations
```

## 🛠️ Kurulum & Geliştirme

### 📋 **Ön Gereksinimler**

- **.NET 8.0 SDK** - [İndir](https://dotnet.microsoft.com/download)
- **PostgreSQL 14+** - [İndir](https://www.postgresql.org/download/)
- **PostGIS Extension** - [Kurulum Rehberi](https://postgis.net/install/)
- **Visual Studio 2022** veya **VS Code** (önerilen)

### 🔧 **Adım 1: Veritabanı Kurulumu**

```bash
# PostgreSQL'de yeni veritabanı oluştur
createdb basarmapdb

# PostGIS extension'ını etkinleştir
psql -d basarmapdb -c "CREATE EXTENSION postgis;"
```

### 🔧 **Adım 2: Connection String Ayarı**

`appsettings.json` dosyasını düzenleyin:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=basarmapdb;Username=YOUR_USERNAME;Password=YOUR_PASSWORD"
  }
}
```

### 🔧 **Adım 3: Veritabanı Migration**

```bash
# Proje dizinine git
cd backend/BasarMapApp.Api

# NuGet paketlerini yükle
dotnet restore

# Migration oluştur (ilk kez)
dotnet ef migrations add InitialCreate

# Veritabanını güncelle
dotnet ef database update
```

### 🔧 **Adım 4: Uygulamayı Çalıştır**

```bash
# Development modunda çalıştır
dotnet run

# Veya watch mode (otomatik reload)
dotnet watch run
```

## 🌐 API Endpoints

### 📍 **Points API**

| Method | Endpoint | Açıklama |
|--------|----------|----------|
| `GET` | `/api/points` | Tüm noktaları listele |
| `GET` | `/api/points/{id}` | ID'ye göre nokta getir |
| `POST` | `/api/points` | Yeni nokta oluştur |
| `PUT` | `/api/points/{id}` | Nokta güncelle |
| `DELETE` | `/api/points/{id}` | Nokta sil |

### 📏 **Lines API**

| Method | Endpoint | Açıklama |
|--------|----------|----------|
| `GET` | `/api/lines` | Tüm çizgileri listele |
| `GET` | `/api/lines/{id}` | ID'ye göre çizgi getir |
| `POST` | `/api/lines` | Yeni çizgi oluştur |
| `PUT` | `/api/lines/{id}` | Çizgi güncelle |
| `DELETE` | `/api/lines/{id}` | Çizgi sil |

### 🔷 **Polygons API**

| Method | Endpoint | Açıklama |
|--------|----------|----------|
| `GET` | `/api/polygons` | Tüm poligonları listele |
| `GET` | `/api/polygons/{id}` | ID'ye göre poligon getir |
| `POST` | `/api/polygons` | Yeni poligon oluştur |
| `PUT` | `/api/polygons/{id}` | Poligon güncelle |
| `DELETE` | `/api/polygons/{id}` | Poligon sil |

## 📝 **Request/Response Örnekleri**

### 📍 **Nokta Oluşturma**

```bash
POST /api/points
Content-Type: application/json

{
  "name": "İstanbul Merkez",
  "description": "Şehir merkez noktası", 
  "latitude": 41.0082,
  "longitude": 28.9784
}
```

**Response:**
```json
{
  "success": true,
  "message": "Point created successfully",
  "data": {
    "id": 1,
    "name": "İstanbul Merkez",
    "description": "Şehir merkez noktası",
    "latitude": 41.0082,
    "longitude": 28.9784,
    "createdAt": "2024-01-15T10:30:00Z"
  }
}
```

### 📏 **Çizgi Oluşturma**

```bash
POST /api/lines
Content-Type: application/json

{
  "name": "İstanbul-Ankara Rotası",
  "description": "Ana ulaşım rotası",
  "coordinates": [
    [28.9784, 41.0082],
    [32.8597, 39.9334]
  ]
}
```

## 🗄️ **Veritabanı Şeması**

### **Points Tablosu**
```sql
CREATE TABLE Points (
    Id SERIAL PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    Description VARCHAR(500),
    Geometry GEOMETRY(POINT, 4326) NOT NULL,
    CreatedAt TIMESTAMP NOT NULL,
    UpdatedAt TIMESTAMP
);
```

### **Lines Tablosu**
```sql
CREATE TABLE Lines (
    Id SERIAL PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    Description VARCHAR(500),
    Geometry GEOMETRY(LINESTRING, 4326) NOT NULL,
    CreatedAt TIMESTAMP NOT NULL,
    UpdatedAt TIMESTAMP
);
```

### **Polygons Tablosu**
```sql
CREATE TABLE Polygons (
    Id SERIAL PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    Description VARCHAR(500),
    Geometry GEOMETRY(POLYGON, 4326) NOT NULL,
    CreatedAt TIMESTAMP NOT NULL,
    UpdatedAt TIMESTAMP
);
```

## 🔧 **Konfigürasyon**

### **CORS Ayarları**
```csharp
// Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
```

### **Dependency Injection**
```csharp
// Repositories
builder.Services.AddScoped<IPointRepository, PointRepository>();
builder.Services.AddScoped<ILineRepository, LineRepository>();
builder.Services.AddScoped<IPolygonRepository, PolygonRepository>();

// Services  
builder.Services.AddScoped<IPointService, PointService>();
builder.Services.AddScoped<ILineService, LineService>();
builder.Services.AddScoped<IPolygonService, PolygonService>();
```

## 🧪 **Test Etme**

### **Swagger UI**
Uygulama çalıştıktan sonra:
```
https://localhost:7XXX/swagger
```

### **HTTP Requests**
`BasarMapApp.Api.http` dosyasını kullanarak test edin:

```http
### Get all points
GET https://localhost:7XXX/api/points
Accept: application/json

### Create a new point  
POST https://localhost:7XXX/api/points
Content-Type: application/json

{
  "name": "Test Point",
  "description": "Test description",
  "latitude": 39.9334,
  "longitude": 32.8597
}
```

## 🛡️ **Validation & Error Handling**

### **Validation Rules**
- **Name**: Zorunlu, max 100 karakter
- **Description**: Opsiyonel, max 500 karakter
- **Coordinates**: Geçerli WGS84 koordinatları
- **Latitude**: -90 ile +90 arasında
- **Longitude**: -180 ile +180 arasında

### **Error Response Format**
```json
{
  "success": false,
  "message": "Validation failed",
  "errors": [
    "Name is required",
    "Latitude must be between -90 and 90"
  ]
}
```

## 📦 **NuGet Paketleri**

| Paket | Versiyon | Açıklama |
|-------|----------|----------|
| `Microsoft.AspNetCore.OpenApi` | 8.0.17 | OpenAPI/Swagger desteği |
| `Microsoft.EntityFrameworkCore.Tools` | 9.0.6 | EF Core CLI araçları |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | 9.0.4 | PostgreSQL provider |
| `Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite` | 9.0.4 | PostGIS/geometri desteği |
| `AutoMapper.Extensions.Microsoft.DependencyInjection` | 12.0.1 | Object mapping |
| `Swashbuckle.AspNetCore` | 6.6.2 | Swagger UI |

## 🏗️ **Clean Architecture Katmanları**

### **Controllers** (Presentation Layer)
- HTTP request/response handling
- Input validation
- Response formatting

### **Services** (Business Logic Layer)  
- İş kuralları ve logic
- Validation
- Koordinasyon

### **Repositories** (Data Access Layer)
- Veritabanı işlemleri
- Entity Framework queries
- Data persistence

### **Models** (Domain Layer)
- Entity tanımları
- Domain logic
- Validation attributes

## 🚀 **Production Deployment**

### **Environment Variables**
```bash
export ASPNETCORE_ENVIRONMENT=Production
export ConnectionStrings__DefaultConnection="your-production-connection-string"
```

### **Docker Support** (Opsiyonel)
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["BasarMapApp.Api.csproj", "."]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BasarMapApp.Api.dll"]
```

## 📚 **Kaynaklar & Dokümantasyon**

- [ASP.NET Core Web API](https://docs.microsoft.com/en-us/aspnet/core/web-api/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [PostGIS Documentation](https://postgis.net/documentation/)
- [NetTopologySuite](https://github.com/NetTopologySuite/NetTopologySuite)
- [AutoMapper](https://automapper.org/)

## 🤝 **Katkıda Bulunma**

1. Bu repo'yu fork edin
2. Feature branch oluşturun (`git checkout -b feature/yeni-ozellik`)
3. Değişikliklerinizi commit edin (`git commit -am 'Yeni özellik eklendi'`)
4. Branch'inizi push edin (`git push origin feature/yeni-ozellik`)
5. Pull Request oluşturun

## 📄 **Lisans**

Bu proje MIT lisansı altında lisanslanmıştır - detaylar için [LICENSE](LICENSE) dosyasına bakın.

## 👤 **İletişim**

Proje Maintainer: [Kürşat E.S. YÜCEL](https://github.com/kursatesyucel) 

Proje Linki: [https://github.com/yourusername/BasarMapApp](https://github.com/yourusername/BasarMapApp)

---
Made with ❤️ by [Kürşat E.S. YÜCEL](https://github.com/kursatesyucel) 