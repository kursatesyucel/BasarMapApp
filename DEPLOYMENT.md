# BasarMapApp - Canlı Ortam (Production) Kurulum Rehberi

Bu rehber, projeyi canlıya almak için adım adım talimatlar içerir.

## Mimari Özeti

| Bileşen | Teknoloji | Açıklama |
|---------|-----------|----------|
| Frontend | React + Vite | Static dosyalar, Nginx ile servis |
| Backend | ASP.NET Core 8 | REST API |
| Veritabanı | PostgreSQL + PostGIS | Harita verileri |
| Log/Device DB | MongoDB | Login logları, cihaz takibi |
| Reverse Proxy | Nginx | TLS, API proxy |

---

## Seçenek 1: Docker ile Tek Sunucuda (Önerilen)

### Gereksinimler
- Sunucu: Ubuntu 22.04 LTS (min 2GB RAM, 2 CPU)
- Docker & Docker Compose kurulu
- Domain adı (örn: app.example.com)
- SSL sertifikası (Let's Encrypt ücretsiz)

### 1.1 docker-compose.production.yml Oluşturma

Mevcut `docker-compose.yml` MongoDB içermiyor. Production için güncellenmiş versiyon:

```yaml
version: '3.8'

services:
  database:
    image: postgis/postgis:15-3.3
    environment:
      POSTGRES_DB: basarmapdb
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: ${DB_PASSWORD}
    volumes:
      - postgres_data:/var/lib/postgresql/data
    restart: unless-stopped
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres"]
      interval: 30s
      timeout: 10s
      retries: 3

  mongodb:
    image: mongo:7
    volumes:
      - mongo_data:/data/db
    restart: unless-stopped
    healthcheck:
      test: ["CMD", "mongosh", "--eval", "db.adminCommand('ping')"]
      interval: 10s
      timeout: 5s
      retries: 5

  backend:
    build:
      context: ./backend
      dockerfile: BasarMapApp.Api/Dockerfile
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:80
      - ConnectionStrings__DefaultConnection=Host=database;Port=5432;Database=basarmapdb;Username=postgres;Password=${DB_PASSWORD}
      - JwtSettings__Secret=${JWT_SECRET}
      - JwtSettings__Issuer=${JWT_ISSUER:-BasarMapApp}
      - JwtSettings__Audience=${JWT_AUDIENCE:-BasarMapAppUsers}
      - MongoDbSettings__ConnectionString=mongodb://mongodb:27017
      - MongoDbSettings__DatabaseName=BasarMapAppLogs
      - MongoDbSettings__UserDevicesCollectionName=UserDevices
      - MongoDbSettings__LoginLogsCollectionName=LoginLogs
      - MongoDbSettings__AuditLogsCollectionName=AuditLogs
      - MailSettings__Email=${MAIL_EMAIL}
      - MailSettings__Password=${MAIL_PASSWORD}
      - MailSettings__Host=${MAIL_HOST:-smtp.gmail.com}
      - MailSettings__Port=${MAIL_PORT:-587}
      - FrontendUrl=${FRONTEND_URL}
    ports:
      - "5000:80"
    depends_on:
      database:
        condition: service_healthy
      mongodb:
        condition: service_healthy
    restart: unless-stopped

  frontend:
    build:
      context: ./frontend
      dockerfile: Dockerfile
      args:
        - VITE_API_BASE_URL=/api
    ports:
      - "3000:80"
    depends_on:
      - backend
    restart: unless-stopped

volumes:
  postgres_data:
  mongo_data:
```

### 1.2 Environment Dosyası (.env.production)

Proje kökünde `.env.production` oluşturun (Git'e eklemeyin!):

```env
DB_PASSWORD=güçlü_şifre_buraya
JWT_SECRET=EnAz32KarakterUzunlugundaGizliBirAnahtarKullanin
MAIL_EMAIL=your-email@gmail.com
MAIL_PASSWORD=uygulama_sifresi
FRONTEND_URL=https://app.example.com
```

**Gmail App Password:** Google Hesabı → Güvenlik → 2 Adımlı Doğrulama → Uygulama şifreleri

### 1.3 Frontend Dockerfile Düzeltmesi

Frontend build için `npm ci --only=production` devDependencies'ı hariç tutar (Vite build için hatalı). Düzeltme gerekli:

```dockerfile
# Build stage
FROM node:18-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
ARG VITE_API_BASE_URL=/api
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL
RUN npm run build

# Production stage
FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/nginx.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

### 1.4 Nginx Reverse Proxy (Sunucuda)

Sunucuya Nginx kurup TLS ile domain'i yönlendirin:

```nginx
# /etc/nginx/sites-available/basarmap
server {
    listen 80;
    server_name app.example.com;
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    server_name app.example.com;

    ssl_certificate /etc/letsencrypt/live/app.example.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/app.example.com/privkey.pem;

    location / {
        proxy_pass http://localhost:3000;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }

    location /api {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

### 1.5 Çalıştırma

```bash
# Sunucuya projeyi kopyala
scp -r BasarMapApp user@sunucu:/home/user/

# Sunucuda
cd /home/user/BasarMapApp
cp .env.production .env
docker compose -f docker-compose.production.yml up -d

# Logları izle
docker compose -f docker-compose.production.yml logs -f
```

---

## Seçenek 2: Bulut PaaS (Render, Railway, Azure)

### Render.com
- **Backend:** Web Service, Docker veya Native .NET
- **Frontend:** Static Site (build: `npm run build`, publish: `dist/`)
- **PostgreSQL:** Render PostgreSQL eklentisi
- **MongoDB:** MongoDB Atlas (ücretsiz tier)

### Azure App Service
- Backend: Azure App Service (Linux, .NET 8)
- Frontend: Azure Static Web Apps veya App Service Static
- PostgreSQL: Azure Database for PostgreSQL
- MongoDB: Azure Cosmos DB (Mongo API) veya MongoDB Atlas

### Railway.app
- Tüm servisleri tek projede deploy edebilirsiniz
- PostgreSQL ve MongoDB plugin'leri mevcut

---

## Seçenek 3: Manuel VPS Kurulumu

### Sunucu Kurulumu (Ubuntu 22.04)

```bash
# Güncellemeler
sudo apt update && sudo apt upgrade -y

# .NET 8 Runtime
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt update
sudo apt install -y aspnetcore-runtime-8.0

# PostgreSQL + PostGIS
sudo apt install -y postgresql postgresql-contrib postgis

# MongoDB
wget -qO - https://www.mongodb.org/static/pgp/server-7.0.asc | sudo apt-key add -
echo "deb [ arch=amd64,arm64 ] https://repo.mongodb.org/apt/ubuntu jammy/mongodb-org/7.0 multiverse" | sudo tee /etc/apt/sources.list.d/mongodb-org-7.0.list
sudo apt update && sudo apt install -y mongodb-org

# Nginx
sudo apt install -y nginx certbot python3-certbot-nginx
```

### Backend Publish

```bash
cd backend/BasarMapApp.Api
dotnet publish -c Release -o ./publish
# publish/ klasörünü sunucuya kopyala
```

### Systemd Service (Backend)

```ini
# /etc/systemd/system/basarmap-api.service
[Unit]
Description=BasarMapApp API
After=network.target postgresql.service mongod.service

[Service]
WorkingDirectory=/opt/basarmap/backend
ExecStart=/usr/bin/dotnet BasarMapApp.Api.dll --urls "http://0.0.0.0:5000"
Restart=always
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ConnectionStrings__DefaultConnection=Host=localhost;...
# Diğer environment değişkenleri

[Install]
WantedBy=multi-user.target
```

---

## Kontrol Listesi

- [ ] JWT Secret production'da güçlü ve benzersiz (min 32 karakter)
- [ ] Veritabanı şifreleri güçlü ve güvende
- [ ] Mail ayarları doğru (Gmail App Password)
- [ ] CORS ayarları production URL'lere göre
- [ ] `appsettings.Production.json` veya env var'lar doğru
- [ ] Frontend build'de `VITE_API_BASE_URL=/api` (aynı domain için)
- [ ] HTTPS zorunlu
- [ ] Yedekleme stratejisi (PostgreSQL, MongoDB)

---

## Sorun Giderme

### Backend 500 Hatası
- Logları kontrol et: `docker logs <container_id>`
- Connection string, MongoDB connection doğru mu?

### Mail Gitmiyor
- Gmail: "Daha az güvenli uygulamalar" kapatıldıysa App Password kullanın
- SMTP port 587 (TLS)

### CORS Hatası
- `Program.cs` production'da `AllowAnyOrigin` kullanıyorsa güvenlik riski
- Sadece `FRONTEND_URL`'e izin veren politika önerilir
