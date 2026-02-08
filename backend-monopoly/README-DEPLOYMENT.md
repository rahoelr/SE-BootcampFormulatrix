# Tutorial Deployment Backend Monopoly ke VPS

Panduan lengkap untuk deploy Backend Monopoly API ke VPS menggunakan Docker dan Nginx.

## Informasi VPS
- **IP Address**: `13.212.217.150`
- **Port**: `8080`
- **URL API**: `http://13.212.217.150:8080`
- **Swagger UI**: `http://13.212.217.150:8080/swagger`

---

## Prasyarat

### 1. Akses ke VPS
Pastikan Anda memiliki:
- SSH access ke VPS (username dan password atau SSH key)
- Koneksi internet yang stabil

### 2. Software yang Dibutuhkan di Komputer Lokal
- Terminal/Command Prompt
- SCP/SFTP client (FileZilla, WinSCP, atau terminal built-in)

---

## Bagian 1: Persiapan VPS

### Step 1: Login ke VPS via SSH

```bash
ssh root@13.212.217.150
# atau jika menggunakan user biasa:
ssh username@13.212.217.150
```

### Step 2: Update System

```bash
sudo apt update
sudo apt upgrade -y
```

### Step 3: Install Docker

```bash
# Install dependencies
sudo apt install -y apt-transport-https ca-certificates curl software-properties-common

# Add Docker GPG key
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /usr/share/keyrings/docker-archive-keyring.gpg

# Add Docker repository
echo "deb [arch=$(dpkg --print-architecture) signed-by=/usr/share/keyrings/docker-archive-keyring.gpg] https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

# Install Docker
sudo apt update
sudo apt install -y docker-ce docker-ce-cli containerd.io

# Verify Docker installation
docker --version
```

### Step 4: Install Docker Compose

```bash
# Download Docker Compose
sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose

# Make it executable
sudo chmod +x /usr/local/bin/docker-compose

# Verify installation
docker-compose --version
```

### Step 5: Configure Docker (Optional but Recommended)

```bash
# Add current user to docker group to run docker without sudo
sudo usermod -aG docker $USER

# Apply group changes (or logout and login again)
newgrp docker

# Test docker without sudo
docker ps
```

### Step 6: Configure Firewall

```bash
# Allow SSH (jangan lupa ini, atau Anda akan kehilangan akses!)
sudo ufw allow 22/tcp

# Allow port 8080 for API
sudo ufw allow 8080/tcp

# Enable firewall
sudo ufw enable

# Check firewall status
sudo ufw status
```

---

## Bagian 2: Upload Project ke VPS

### Opsi A: Menggunakan SCP (dari komputer lokal)

```bash
# Buka terminal di komputer lokal, lalu navigasi ke folder project
cd /Users/rahoolll/Bootcamp-Formulatrix/SE-BootcampFormulatrix/backend-monopoly

# Upload seluruh folder project ke VPS
scp -r . root@13.212.217.150:/opt/monopoly-backend
# atau jika menggunakan user biasa:
scp -r . username@13.212.217.150:~/monopoly-backend
```

### Opsi B: Menggunakan Git (jika project di GitHub)

```bash
# Di VPS, clone repository
cd /opt
sudo git clone https://github.com/your-username/backend-monopoly.git
cd backend-monopoly
```

### Opsi C: Menggunakan FileZilla/WinSCP (GUI)

1. Buka FileZilla/WinSCP
2. Host: `13.212.217.150`
3. Username: `root` (atau username Anda)
4. Password: (password VPS Anda)
5. Port: `22`
6. Upload seluruh folder project

---

## Bagian 3: Deploy Aplikasi

### Step 1: Navigasi ke Folder Project

```bash
cd /opt/monopoly-backend
# atau
cd ~/monopoly-backend
```

### Step 2: Verify File Structure

```bash
ls -la

# Pastikan ada file berikut:
# - Dockerfile
# - docker-compose.yml
# - deploy.sh
# - nginx/nginx.conf
# - nginx/conf.d/default.conf
# - appsettings.Production.json
```

### Step 3: Run Deployment Script

```bash
# Make script executable (jika belum)
chmod +x deploy.sh

# Run deployment
./deploy.sh
```

**ATAU Deploy Manual:**

```bash
# Stop existing containers (jika ada)
docker-compose down

# Build Docker image
docker-compose build

# Start containers
docker-compose up -d

# Check container status
docker-compose ps
```

### Step 4: Verify Deployment

```bash
# Check logs
docker-compose logs -f

# Test API locally on VPS
curl http://localhost:8080

# Check container health
docker ps
```

---

## Bagian 4: Testing API

### Dari Browser

Buka browser dan akses:
- **API Base**: `http://13.212.217.150:8080`
- **Swagger UI**: `http://13.212.217.150:8080/swagger`

### Dari Terminal (curl)

```bash
# Test dari komputer lokal
curl http://13.212.217.150:8080

# Test specific endpoint
curl http://13.212.217.150:8080/api/game
```

### Dari Postman

1. Buka Postman
2. Create New Request
3. URL: `http://13.212.217.150:8080/api/game`
4. Method: GET/POST (sesuai endpoint)
5. Send

---

## Bagian 5: Management & Monitoring

### View Logs

```bash
# View all logs
docker-compose logs

# Follow logs (real-time)
docker-compose logs -f

# View specific service logs
docker-compose logs backend-api
docker-compose logs nginx

# View last 100 lines
docker-compose logs --tail=100
```

### Container Management

```bash
# Check status
docker-compose ps

# Stop containers
docker-compose stop

# Start containers
docker-compose start

# Restart containers
docker-compose restart

# Stop and remove containers
docker-compose down

# View resource usage
docker stats
```

### Update Aplikasi

```bash
# Pull latest code (jika menggunakan Git)
git pull origin main

# Rebuild and restart
docker-compose down
docker-compose build
docker-compose up -d

# ATAU gunakan script deploy
./deploy.sh
```

---

## Bagian 6: Troubleshooting

### Problem 1: Container tidak bisa start

```bash
# Check logs untuk error
docker-compose logs backend-api

# Check port conflicts
sudo netstat -tulpn | grep 8080
sudo netstat -tulpn | grep 5000

# Remove old containers
docker-compose down -v
docker system prune -a
```

### Problem 2: API tidak bisa diakses dari luar

```bash
# Check firewall
sudo ufw status

# Allow port 8080
sudo ufw allow 8080/tcp

# Check if container is running
docker ps

# Check nginx logs
docker-compose logs nginx
```

### Problem 3: Build gagal

```bash
# Check disk space
df -h

# Clean Docker cache
docker system prune -a

# Rebuild from scratch
docker-compose build --no-cache
```

### Problem 4: Out of Memory

```bash
# Check memory usage
free -h

# Restart Docker
sudo systemctl restart docker

# Limit container memory (edit docker-compose.yml)
# Add under backend-api service:
#   mem_limit: 512m
```

### Problem 5: Permission denied

```bash
# Fix ownership
sudo chown -R $USER:$USER .

# Fix deploy.sh permissions
chmod +x deploy.sh
```

---

## Bagian 7: Security Best Practices

### 1. Update CORS untuk Production

Edit `appsettings.Production.json` dan ubah allowed origins sesuai frontend Anda:

```json
"CORS": {
  "AllowedOrigins": [
    "https://your-frontend-domain.com"
  ]
}
```

### 2. Enable HTTPS (jika punya domain)

Install Certbot dan Let's Encrypt:

```bash
sudo apt install certbot python3-certbot-nginx
sudo certbot --nginx -d your-domain.com
```

### 3. Change Default SSH Port

```bash
# Edit SSH config
sudo nano /etc/ssh/sshd_config

# Change: Port 22 -> Port 2222
# Restart SSH
sudo systemctl restart sshd
```

### 4. Install Fail2Ban

```bash
sudo apt install fail2ban
sudo systemctl enable fail2ban
sudo systemctl start fail2ban
```

---

## Bagian 8: Backup & Recovery

### Backup Container Data

```bash
# Backup volumes
docker-compose down
sudo tar -czf monopoly-backup-$(date +%Y%m%d).tar.gz /opt/monopoly-backend

# Copy to local
scp root@13.212.217.150:/opt/monopoly-backup-*.tar.gz ./
```

### Auto-restart on Boot

```bash
# Docker containers will auto-restart (already configured in docker-compose.yml)
# Verify restart policy:
docker inspect monopoly-backend | grep -A 5 RestartPolicy
```

---

## Bagian 9: Performance Optimization

### Monitor Resource Usage

```bash
# Install htop
sudo apt install htop
htop

# Docker stats
docker stats

# Disk usage
docker system df
```

### Clean Up Unused Resources

```bash
# Remove unused images
docker image prune -a

# Remove unused containers
docker container prune

# Remove unused volumes
docker volume prune

# Clean everything
docker system prune -a --volumes
```

---

## Useful Commands Cheat Sheet

```bash
# Deployment
./deploy.sh                          # Deploy aplikasi
docker-compose up -d                 # Start containers
docker-compose down                  # Stop containers
docker-compose restart              # Restart containers

# Logs
docker-compose logs -f              # Follow logs
docker-compose logs --tail=100      # Last 100 lines
docker-compose logs backend-api     # Specific service

# Status
docker-compose ps                   # Container status
docker ps                           # All containers
docker stats                        # Resource usage

# Maintenance
docker system prune -a              # Clean unused resources
docker-compose build --no-cache     # Rebuild from scratch
docker-compose down -v              # Stop and remove volumes

# Monitoring
curl http://localhost:8080          # Test API
netstat -tulpn | grep 8080         # Check port usage
```

---

## Kontak & Support

Jika ada masalah:
1. Check logs: `docker-compose logs -f`
2. Check container status: `docker-compose ps`
3. Restart services: `docker-compose restart`
4. Rebuild if needed: `docker-compose build --no-cache`

---

## Summary

Aplikasi Anda sekarang berjalan di:
- **API**: http://13.212.217.150:8080
- **Swagger**: http://13.212.217.150:8080/swagger

Selamat! Backend Monopoly API Anda sudah berhasil di-deploy! 🎉
