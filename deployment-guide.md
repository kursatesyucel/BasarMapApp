# 🚀 BasarMapApp Deployment Rehberi

Bu rehber BasarMapApp projenizi canlıya almak için gerekli adımları açıklar.

## 📋 Ön Koşullar

- Docker ve Docker Compose yüklü olmalı
- Git yüklü olmalı
- Cloud provider hesabı (Azure, AWS, GCP, Railway, vb.)

## 🐳 Docker ile Local Test

Önce local ortamda test edin:

```bash
# Projeyi klonlayın (eğer git repo'su ise)
git clone <your-repo-url>
cd BasarMapApp

# Docker compose ile başlatın
docker-compose up --build

# Uygulamaya erişin:
# Frontend: http://localhost:3000
# Backend API: http://localhost:5000
# Swagger: http://localhost:5000/swagger
```

## ☁️ Cloud Deployment Seçenekleri

### 1. Railway (En Kolay) ⭐ ÖNERİLEN

```bash
# Railway CLI'yı yükleyin
npm install -g @railway/cli

# Railway'e login olun
railway login

# Projeyi deploy edin
railway up
```

### 2. Azure Container Instances

```bash
# Azure CLI ile login
az login

# Resource group oluşturun
az group create --name basarmapapp-rg --location "West Europe"

# Container registry oluşturun
az acr create --resource-group basarmapapp-rg --name basarmapappregistry --sku Basic

# Docker images'ları build edin ve push edin
docker build -t basarmapapp-backend ./backend
docker build -t basarmapapp-frontend ./frontend

# Deploy edin
az container create --resource-group basarmapapp-rg --name basarmapapp ...
```

### 3. AWS ECS/Fargate

```bash
# AWS CLI konfigürasyonu
aws configure

# ECR repository oluşturun
aws ecr create-repository --repository-name basarmapapp-backend
aws ecr create-repository --repository-name basarmapapp-frontend

# Images'ları push edin ve ECS service oluşturun
```

### 4. Google Cloud Run

```bash
# Google Cloud SDK kurulumu sonrası
gcloud auth login
gcloud config set project YOUR_PROJECT_ID

# Build ve deploy
gcloud builds submit --tag gcr.io/YOUR_PROJECT_ID/basarmapapp-backend ./backend
gcloud run deploy --image gcr.io/YOUR_PROJECT_ID/basarmapapp-backend --platform managed
```

## 🔧 Production Ayarları

### Environment Variables

Aşağıdaki environment variable'ları ayarlayın:

```bash
# Database
DB_PASSWORD=güçlü_şifreniz

# API Base URL (Frontend için)
VITE_API_BASE_URL=https://your-backend-url.com/api
```

### Database Migration

Production'da veritabanı migration'ları çalıştırın:

```bash
# Backend container'da
dotnet ef database update --environment Production
```

### SSL/HTTPS

Production ortamında mutlaka HTTPS kullanın:

- Let's Encrypt sertifikaları
- Cloud provider SSL sertifikaları
- Reverse proxy (nginx/cloudflare)

## 📱 Frontend Konfigürasyonu

`frontend/src/services/api.ts` dosyasında API base URL'ini environment variable'dan alacak şekilde güncelleyin:

```typescript
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api';
```

## 🔐 Güvenlik Checklist

- [ ] Database şifrelerini güçlü yapın
- [ ] API rate limiting ekleyin
- [ ] HTTPS zorunlu hale getirin
- [ ] Environment variable'larda sensitive data
- [ ] CORS ayarlarını production için kısıtlayın
- [ ] Input validation'ları kontrol edin

## 📊 Monitoring

Production'da izleme için:

- Application Insights (Azure)
- CloudWatch (AWS)
- Stackdriver (GCP)
- Sentry.io (Error tracking)

## 🔄 CI/CD Pipeline

GitHub Actions örneği:

```yaml
name: Deploy to Production

on:
  push:
    branches: [main]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2
      - name: Deploy to Railway
        run: railway up
```

## 🆘 Troubleshooting

### Sık Karşılaşılan Sorunlar:

1. **CORS Hatası**: Program.cs'deki CORS ayarlarını kontrol edin
2. **Database Connection**: Connection string'i ve PostgreSQL versiyonunu kontrol edin
3. **PostGIS**: Veritabanında PostGIS extension'ının yüklü olduğundan emin olun
4. **SSL**: Production'da HTTPS redirect'leri aktif olmalı

### Log'ları İnceleme:

```bash
# Docker container log'ları
docker-compose logs -f backend
docker-compose logs -f frontend
docker-compose logs -f database
```

## 📞 Destek

Deployment sırasında sorun yaşarsanız:
1. Log dosyalarını kontrol edin
2. Environment variable'ları doğrulayın
3. Network bağlantısını test edin
4. Cloud provider documentation'ını inceleyin 