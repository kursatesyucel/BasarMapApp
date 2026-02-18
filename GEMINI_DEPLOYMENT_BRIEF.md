# BasarMapApp - Gemini için Canlıya Alma Özeti

Bu doküman, BasarMapApp projesini canlıya almak için Gemini'nin bilmesi gereken tüm bilgileri içerir.

---

## 1. Proje Özeti

**BasarMapApp** — Harita tabanlı bir web uygulaması. Kullanıcılar giriş yapıp harita üzerinde nokta, çizgi ve poligon ekleyebilir; iller, ilçeler ve yerleşimler ile sınır verilerini görüntüleyebilir.

---

## 2. Teknoloji Yığını

| Katman | Teknoloji |
|--------|-----------|
| **Frontend** | React 19 + Vite 7 + TypeScript + Leaflet + React-Leaflet |
| **Backend** | ASP.NET Core 8 (REST API) |
| **Ana DB** | PostgreSQL 15 + PostGIS 3.3 |
| **İkincil DB** | MongoDB 7 (loglar, cihaz takibi) |
| **Auth** | JWT Bearer |
| **Mail** | SMTP (Gmail) |

---

## 3. Proje Yapısı

```
BasarMapApp/
├── frontend/                 # React SPA
│   ├── src/
│   │   ├── components/       # UI bileşenleri
│   │   ├── pages/            # Sayfa bileşenleri
│   │   ├── services/         # API servisleri
│   │   ├── hooks/
│   │   └── utils/
│   ├── Dockerfile
│   ├── nginx.conf            # /api → backend proxy
│   └── vite.config.js
├── backend/
│   ├── BasarMapApp.Api/
│   │   ├── Controllers/      # 8 controller
│   │   ├── Services/
│   │   ├── Repositories/
│   │   ├── Models/
│   │   ├── Data/
│   │   │   ├── ApplicationDbContext.cs
│   │   │   └── DbSeeder.cs   # admin kullanıcı seed
│   │   ├── Migrations/
│   │   ├── Dockerfile
│   │   └── appsettings.json
│   └── SQL/                  # GIS indeksleri, örnekler
├── docker-compose.production.yml
├── .env.production.example
└── DEPLOYMENT.md
```

---

## 4. Kritik Konfigürasyonlar

### 4.1 Backend Environment Variables (Zorunlu)

| Değişken | Açıklama | Örnek |
|----------|----------|-------|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string | `Host=database;Port=5432;Database=basarmapdb;Username=postgres;Password=XXX` |
| `JwtSettings__Secret` | JWT imza anahtarı (min 32 karakter) | Güçlü rastgele string |
| `JwtSettings__Issuer` | JWT issuer | `BasarMapApp` |
| `JwtSettings__Audience` | JWT audience | `BasarMapAppUsers` |
| `MongoDbSettings__ConnectionString` | MongoDB bağlantısı | `mongodb://mongodb:27017` |
| `MongoDbSettings__DatabaseName` | MongoDB veritabanı | `BasarMapAppLogs` |
| `MongoDbSettings__UserDevicesCollectionName` | Cihaz koleksiyonu | `UserDevices` |
| `MongoDbSettings__LoginLogsCollectionName` | Login log koleksiyonu | `LoginLogs` |
| `MongoDbSettings__AuditLogsCollectionName` | Audit log koleksiyonu | `AuditLogs` |
| `MailSettings__Email` | SMTP gönderici email | Gmail adresi |
| `MailSettings__Password` | Gmail uygulama şifresi | App Password (normal şifre çalışmaz) |
| `MailSettings__Host` | SMTP host | `smtp.gmail.com` |
| `MailSettings__Port` | SMTP port | `587` |
| `FrontendUrl` | Frontend base URL (CORS, mail linkleri) | `https://app.example.com` |

### 4.2 Frontend Build Argümanı

- `VITE_API_BASE_URL` — Production build için `/api` olmalı (aynı domain üzerinden proxy).

### 4.3 appsettings.Production.json Durumu

- Şu an sadece `ConnectionStrings` tanımlı.
- **JwtSettings, MongoDbSettings, MailSettings** production ortamında environment variables üzerinden gelmeli.
- `Program.cs` içinde `builder.Configuration["JwtSettings:Secret"]` null/boş ise exception fırlatıyor.

---

## 5. Veritabanları

### 5.1 PostgreSQL (ApplicationDbContext)

**EF Core Migration'lar:**
- `AddUserAuthenticationV2`
- `AddEmailVerificationV2`
- `AddIsActiveToUser`
- `AddBruteForceProtectionToUser`
- `AddPasswordResetToUser`

**Tablolar:** Points, Lines, Polygons, Cameras, Users + GIS referans tabloları

**İlk çalıştırmada:**
- `dotnet ef database update` veya uygulama başlarken migration otomatik uygulanmalı.
- `DbSeeder.SeedAdminUser()` **sadece Development** ortamında çalışıyor (`Program.cs` satır 169–176). Production için admin kullanıcı manuel oluşturulmalı veya seed Production'a da eklenmeli.

**Admin varsayılan bilgileri (DbSeeder):**
- Username: `admin`
- Email: `admin@basarmapapp.com`
- Password: `Admin123!`
- Role: `Admin`

### 5.2 GIS Referans Tabloları (PostgreSQL)

Uygulama şu tablolara ihtiyaç duyuyor (EF migration ile gelmez):

- `ref_provinces` — İl sınırları (ogc_fid, detay_adı, geom)
- `ref_districts` — İlçe sınırları
- `ref_settlements` — Yerleşim sınırları

Bu tablolar dış kaynak (Shapefile, GeoJSON vb.) ile import edilmeli. Boş bırakılırsa Boundaries API hata verebilir.

### 5.3 MongoDB

- **Otomatik:** Collection'lar ilk istekte oluşur.
- **Collection'lar:** LoginLogs, AuditLogs, UserDevices
- **Index'ler:** DeviceService ilk çalışmada UserDevices için index oluşturur.

---

## 6. API Endpoints

| Endpoint | Auth | Açıklama |
|----------|------|----------|
| `POST /api/auth/login` | Hayır | Giriş |
| `POST /api/auth/register` | Hayır | Kayıt |
| `POST /api/auth/verify-email` | Hayır | Email doğrulama |
| `POST /api/auth/forgot-password` | Hayır | Şifre sıfırlama |
| `POST /api/auth/reset-password` | Hayır | Şifre yenileme |
| `GET /api/points` | JWT | Noktalar |
| `GET /api/lines` | JWT | Çizgiler |
| `GET /api/polygons` | JWT | Poligonlar |
| `GET /api/cameras` | JWT | Kameralar |
| `GET /api/devices` | JWT | Kullanıcı cihazları |
| `PATCH /api/devices/{id}/trusted` | JWT | Güvenli cihaz ayarla |
| `DELETE /api/devices/{id}` | JWT | Cihaz kaldır |
| `GET /api/boundaries/...` | JWT | İl/ilçe/yerleşim sınırları |
| `GET /api/admin/logs` | JWT + Admin | Loglar |

**Önemli:** İsteklerde `X-Device-Id` header'ı gönderilir (frontend `deviceFingerprint` ile üretir).

---

## 7. CORS

- **Development:** `AllowAnyOrigin()`
- **Production:** Şu an `AllowAnyOrigin()` — güvenlik açısından `WithOrigins(FrontendUrl)` ile sınırlanması önerilir.

---

## 8. Frontend Routing

- SPA: Tüm route'lar `index.html`'e yönlenmeli.
- `nginx.conf` içinde `try_files $uri $uri/ /index.html;` kullanılıyor.

---

## 9. Docker Compose Production

`docker-compose.production.yml`:

- **database:** postgis/postgis:15-3.3 (port 5432)
- **mongodb:** mongo:7
- **backend:** Port 5000 (internal 80)
- **frontend:** Port 3000 (internal 80), `/api` → `backend:80` proxy

**Frontend container:** Nginx içinde `/api` isteklerini `backend:80`'e proxy ediyor. Bu yüzden frontend ve backend aynı docker network'te olmalı.

**Dış erişim:** Reverse proxy (Nginx/Traefik) ile:
- `https://domain.com/` → frontend:3000
- `https://domain.com/api` → backend:5000

---

## 10. Bilinen Sorunlar ve Dikkat Edilecekler

1. **Admin seed sadece Development'ta:** Production'da admin yoksa manuel oluşturulmalı.
2. **GIS tabloları:** `ref_provinces`, `ref_districts`, `ref_settlements` migration ile gelmiyor; import gerekebilir.
3. **appsettings.json:** Git'te ignore edilmiş olabilir; örnek: `appsettings.template.json`.
4. **Gmail:** "Uygulama şifresi" gerekli, normal şifre kabul edilmez.
5. **JWT Secret:** Production'da mutlaka güçlü ve benzersiz olmalı.
6. **Backend port:** `launchSettings.json`'da `0.0.0.0:5012` — Docker'da 80 kullanılıyor.
7. **Frontend Dockerfile:** `VITE_API_BASE_URL` build arg olarak geçilmeli; `npm ci` (production flag olmadan) kullanılmalı.

---

## 11. Deployment Checklist (Gemini için)

- [ ] `.env.production` oluştur (DB_PASSWORD, JWT_SECRET, MAIL_EMAIL, MAIL_PASSWORD, FRONTEND_URL)
- [ ] `appsettings.Production.json` güncelle veya tüm ayarları env var ile sağla
- [ ] PostgreSQL migration'ları çalıştır
- [ ] GIS referans tablolarını import et (varsa)
- [ ] Admin kullanıcı oluştur (Production'da seed yoksa)
- [ ] Reverse proxy: `/` → frontend, `/api` → backend
- [ ] HTTPS (Let's Encrypt vb.) etkinleştir
- [ ] CORS'u production frontend URL'ine göre kısıtla (isteğe bağlı güvenlik iyileştirmesi)

---

## 12. İlgili Dosyalar

- `DEPLOYMENT.md` — Detaylı deployment rehberi
- `docker-compose.production.yml` — Production Docker Compose
- `.env.production.example` — Environment değişkenleri şablonu
- `backend/BasarMapApp.Api/Program.cs` — DI, CORS, middleware
- `frontend/nginx.conf` — API proxy config
- `frontend/src/services/api.ts` — `VITE_API_BASE_URL` kullanımı
