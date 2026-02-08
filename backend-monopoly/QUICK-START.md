# Quick Start - Deployment Guide

## VPS Information
- **IP**: 13.212.217.150
- **Port**: 8080
- **API URL**: http://13.212.217.150:8080
- **Swagger**: http://13.212.217.150:8080/swagger

## Quick Commands

### Di VPS (Setup Pertama Kali)
```bash
# Install Docker & Docker Compose
sudo apt update && sudo apt upgrade -y
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh
sudo usermod -aG docker $USER
sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose
```

### Upload Project ke VPS (dari komputer lokal)
```bash
# Navigasi ke folder project
cd /Users/rahoolll/Bootcamp-Formulatrix/SE-BootcampFormulatrix/backend-monopoly

# Upload ke VPS
scp -r . root@13.212.217.150:/opt/monopoly-backend
```

### Deploy Aplikasi (di VPS)
```bash
# Masuk ke folder project
cd /opt/monopoly-backend

# Deploy dengan script otomatis
chmod +x deploy.sh
./deploy.sh

# ATAU deploy manual
docker-compose build
docker-compose up -d
```

## Management Commands

```bash
# View logs
docker-compose logs -f

# Restart services
docker-compose restart

# Stop services
docker-compose down

# Update aplikasi
git pull  # jika menggunakan git
docker-compose build
docker-compose up -d

# Check status
docker-compose ps
docker stats
```

## Testing

```bash
# Test dari VPS
curl http://localhost:8080

# Test dari komputer lokal
curl http://13.212.217.150:8080

# Browser
http://13.212.217.150:8080/swagger
```

## Troubleshooting

```bash
# Check logs jika error
docker-compose logs backend-api
docker-compose logs nginx

# Restart jika hang
docker-compose restart

# Clean rebuild jika masalah
docker-compose down
docker system prune -a
docker-compose build --no-cache
docker-compose up -d
```

## Files Created
✓ Dockerfile - Multi-stage build untuk .NET 10
✓ docker-compose.yml - Container orchestration
✓ nginx/nginx.conf - Main nginx config
✓ nginx/conf.d/default.conf - Reverse proxy config
✓ appsettings.Production.json - Production settings
✓ .dockerignore - Build optimization
✓ deploy.sh - Automated deployment script
✓ README-DEPLOYMENT.md - Full tutorial

Lihat README-DEPLOYMENT.md untuk tutorial lengkap!
