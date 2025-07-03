# 🗺️ BasarMapApp - Interactive Map Application

[![React](https://img.shields.io/badge/React-18.0-61DAFB?style=flat&logo=react)](https://reactjs.org/)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet)](https://dotnet.microsoft.com/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.0-3178C6?style=flat&logo=typescript)](https://www.typescriptlang.org/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-14+-336791?style=flat&logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![PostGIS](https://img.shields.io/badge/PostGIS-3.0+-FF6600?style=flat&logo=postgis&logoColor=white)](https://postgis.net/)

Modern ve interaktif harita uygulaması. Coğrafi verileri görselleştirme, düzenleme ve yönetme için geliştirilmiş **full-stack** çözüm.

## 🌟 Özellikler

### 🎯 **Geometrik Veri Yönetimi**
- **📍 Points**: Nokta bazlı konum işaretleri
- **📏 Lines**: Çizgi/rota planlaması
- **🔷 Polygons**: Alan/bölge tanımlama
- **📐 Measurement**: Otomatik uzunluk ve alan hesaplaması

### 🖱️ **İnteraktif Harita Özellikleri**
- **✏️ Drag & Drop Editing**: Gerçek zamanlı geometri düzenleme
- **🎨 Visual Drawing Tools**: Leaflet.Draw entegrasyonu
- **🔍 Zoom & Pan**: Smooth harita navigasyonu
- **📱 Responsive Design**: Tüm cihazlarda uyumlu

### 💾 **Veri Yönetimi**
- **🔄 Real-time Sync**: Anlık veritabanı güncelleme
- **📝 CRUD Operations**: Complete data management
- **✅ Validation**: Güçlü veri doğrulama
- **🗃️ PostGIS**: Profesyonel coğrafi veri depolama

## 🏗️ Teknoloji Stack

### 🎨 **Frontend**
- **React 18** + **TypeScript** - Modern UI framework
- **Leaflet** + **Leaflet.Draw** - Interactive mapping
- **Vite** - Lightning fast build tool
- **Axios** - HTTP client
- **CSS3** - Responsive styling

### ⚙️ **Backend**
- **.NET 8.0** Web API - High-performance backend
- **Entity Framework Core** - ORM
- **PostgreSQL** + **PostGIS** - Spatial database
- **AutoMapper** - Object mapping
- **Swagger/OpenAPI** - API documentation

### 🛠️ **Architecture**
- **Clean Architecture** - Scalable code organization
- **Repository Pattern** - Data access abstraction
- **Service Layer** - Business logic separation
- **RESTful API** - Standard web services

## 📸 Screenshots

### 🗺️ **Ana Harita Görünümü**
```
┌─────────────────────────────────────────────────────────────┐
│ 🗺️ Interactive Map                    │ 📋 Feature Panel  │
│                                       │                    │
│  📍 Points    📏 Lines    🔷 Polygons │ 📍 Points (3)      │
│                                       │ • İstanbul         │
│  🎨 Drawing Tools:                    │ • Ankara           │
│  ✏️ Edit  🗑️ Delete  ➕ Add          │ • İzmir            │
│                                       │                    │
│                                       │ 📏 Lines (2)       │
│  [Interactive OpenStreetMap]          │ • İst-Ank Route    │
│                                       │ • Coastal Line     │
│                                       │                    │
│                                       │ 🔷 Polygons (1)    │
│                                       │ • Turkey Border    │
└─────────────────────────────────────────────────────────────┘
```

## 🚀 Hızlı Başlangıç

### 📋 **Ön Gereksinimler**

- **Node.js 18+** - [İndir](https://nodejs.org/)
- **.NET 8.0 SDK** - [İndir](https://dotnet.microsoft.com/download)
- **PostgreSQL 14+** - [İndir](https://www.postgresql.org/download/)
- **PostGIS Extension** - [Kurulum](https://postgis.net/install/)

### 🛠️ **Kurulum Adımları**

#### 1️⃣ **Repository Clone**
```bash
git clone https://github.com/your-username/BasarMapApp.git
cd BasarMapApp
```

#### 2️⃣ **Veritabanı Kurulumu**
```bash
# PostgreSQL'de veritabanı oluştur
createdb basarmapdb

# PostGIS extension'ını etkinleştir  
psql -d basarmapdb -c "CREATE EXTENSION postgis;"
```

#### 3️⃣ **Backend Kurulumu**
```bash
cd backend/BasarMapApp.Api

# appsettings.json'da connection string'i düzenle
# "DefaultConnection": "Host=localhost;Port=5432;Database=basarmapdb;Username=YOUR_USERNAME;Password=YOUR_PASSWORD"

# NuGet paketleri yükle
dotnet restore

# Database migration
dotnet ef database update

# Backend'i çalıştır
dotnet run
```

#### 4️⃣ **Frontend Kurulumu**
```bash
cd frontend

# Dependencies yükle
npm install

# Development server başlat
npm run dev
```

### 🌐 **Erişim Adresleri**

- **Frontend**: http://localhost:5173
- **Backend API**: https://localhost:7XXX
- **Swagger UI**: https://localhost:7XXX/swagger

## 📁 Proje Yapısı

```
BasarMapApp/
├── 📁 frontend/                 # React TypeScript frontend
│   ├── 📁 src/
│   │   ├── 📁 components/       # React components
│   │   │   ├── MapView.tsx      # Main map component
│   │   │   ├── Sidebar.tsx      # Feature management panel
│   │   │   ├── EditFeatureModal.tsx
│   │   │   └── FeatureForm.tsx
│   │   ├── 📁 services/         # API service layer
│   │   ├── 📁 hooks/            # Custom React hooks
│   │   ├── 📁 types/            # TypeScript definitions
│   │   └── 📁 utils/            # Utility functions
│   ├── package.json
│   └── vite.config.js
├── 📁 backend/                  # .NET 8.0 Web API
│   └── 📁 BasarMapApp.Api/
│       ├── 📁 Controllers/      # API endpoints
│       ├── 📁 Models/           # Entity models
│       ├── 📁 Services/         # Business logic
│       ├── 📁 Repositories/     # Data access
│       ├── 📁 DTOs/             # Data transfer objects
│       ├── 📁 Data/             # EF Core context
│       └── 📁 Migrations/       # Database migrations
└── README.md                    # Bu dosya
```

## 🎮 Kullanım Rehberi

### 📍 **Nokta (Point) Oluşturma**
1. Harita üzerinde **marker** aracını seçin
2. Harita üzerinde bir konuma tıklayın
3. Açılan formda **name** ve **description** girin
4. **Save** butonuna tıklayın

### 📏 **Çizgi (Line) Çizme**
1. **Polyline** aracını seçin
2. Başlangıç noktasına tıklayın
3. Ara noktaları ekleyin
4. Son noktada çift tıklayarak tamamlayın
5. Form bilgilerini doldurun

### 🔷 **Alan (Polygon) Tanımlama**
1. **Polygon** aracını seçin
2. Köşe noktalarını sırayla işaretleyin
3. İlk noktaya geri dönerek kapatın
4. Form bilgilerini doldurun

### ✏️ **Düzenleme (Edit)**
1. **Edit** modunu etkinleştirin (kalem ikonu)
2. Düzenlemek istediğin nesneyi seç
3. **Drag & drop** ile noktaları taşı
4. Değişiklikler **otomatik kaydedilir**

### 📝 **Detay Düzenleme**
1. Sidebar'dan bir feature seçin
2. **Edit** butonuna tıklayın
3. Modal'da bilgileri düzenleyin
4. **Save** ile kaydedin

## 🔧 API Referansı

### **Points Endpoints**
```http
GET    /api/points          # Tüm noktaları listele
GET    /api/points/{id}     # Belirli nokta detayı
POST   /api/points          # Yeni nokta oluştur
PUT    /api/points/{id}     # Nokta güncelle
DELETE /api/points/{id}     # Nokta sil
```

### **Request Example**
```json
POST /api/points
{
  "name": "İstanbul Merkez",
  "description": "Şehir merkez noktası",
  "latitude": 41.0082,
  "longitude": 28.9784
}
```

### **Response Example**
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

Detaylı API dokümantasyonu için: **[Backend README](backend/README.md)**

## 🧪 Test Etme

### **Frontend Test**
```bash
cd frontend
npm run test        # Unit tests
npm run build       # Production build test
```

### **Backend Test**
```bash
cd backend/BasarMapApp.Api
dotnet test         # Unit tests
dotnet run          # Development run
```

### **API Test**
- Swagger UI kullanarak: `https://localhost:7XXX/swagger`
- HTTP dosyası ile: `backend/BasarMapApp.Api/BasarMapApp.Api.http`

## 🚀 Production Deployment

### **Frontend (Vercel/Netlify)**
```bash
cd frontend
npm run build
# dist/ klasörünü deploy et
```

### **Backend (Azure/AWS)**
```bash
cd backend/BasarMapApp.Api
dotnet publish -c Release
# Publish output'unu server'a deploy et
```

### **Docker (Opsiyonel)**
```dockerfile
# Dockerfile örneği backend README'de mevcut
docker build -t basarmapapp-backend .
docker build -t basarmapapp-frontend .
```

## 🤝 Katkıda Bulunma

1. Bu repo'yu **fork** edin
2. Feature branch oluşturun: `git checkout -b feature/yeni-ozellik`
3. Değişikliklerinizi commit edin: `git commit -am 'Yeni özellik eklendi'`
4. Branch'inizi push edin: `git push origin feature/yeni-ozellik`
5. **Pull Request** oluşturun

### **Development Guidelines**
- **TypeScript** strict mode kullanın
- **ESLint** rules'larına uyun
- **Clean Architecture** prensiplerini takip edin
- **Unit test** yazın
- **Commit message** standartlarına uyun

## 📄 Lisans

Bu proje **MIT** lisansı altında lisanslanmıştır - detaylar için [LICENSE](LICENSE) dosyasına bakın.

## 🐛 Bug Report & Feature Request

- **Bug Report**: [Issues](https://github.com/your-username/BasarMapApp/issues) sayfasından bildirin
- **Feature Request**: [Discussions](https://github.com/your-username/BasarMapApp/discussions) üzerinden önerin

## 👤 İletişim

**Proje Maintainer**: [Kürşat E.S. YÜCEL](https://github.com/kursatesyucel) 
**Email**: eyucel239@gmail.com 
**LinkedIn**: [Kürşat Yücel](www.linkedin.com/in/kursat-y)

**Proje Linki**: [https://github.com/yourusername/BasarMapApp](https://github.com/yourusername/BasarMapApp)

## 🙏 Teşekkürler

- [OpenStreetMap](https://www.openstreetmap.org/) - Harita verileri
- [Leaflet](https://leafletjs.com/) - Mapping library
- [React](https://reactjs.org/) - Frontend framework
- [.NET](https://dotnet.microsoft.com/) - Backend framework
- [PostGIS](https://postgis.net/) - Spatial database

---

<div align="center">

**⭐ Bu projeyi beğendiyseniz yıldız vermeyi unutmayın!**

Made with ❤️ by [Kürşat E.S. YÜCEL](https://github.com/kursatesyucel)

</div> 