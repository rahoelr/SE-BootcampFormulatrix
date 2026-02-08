# Deployment Checklist

## ✅ Pre-Deployment Checklist

### Local Machine
- [ ] Semua file deployment sudah dibuat
- [ ] Code sudah di-test dan berjalan dengan baik
- [ ] Tidak ada error saat compile

### VPS Preparation
- [ ] VPS bisa diakses via SSH
- [ ] Docker sudah terinstall
- [ ] Docker Compose sudah terinstall
- [ ] Port 8080 sudah dibuka di firewall
- [ ] Disk space minimal 2GB tersedia

## 📋 Deployment Steps

### Step 1: Upload Project
```bash
# Dari komputer lokal
cd /Users/rahoolll/Bootcamp-Formulatrix/SE-BootcampFormulatrix/backend-monopoly
scp -r . root@13.212.217.150:/opt/monopoly-backend
```
- [ ] Upload selesai tanpa error
- [ ] Semua file ter-upload dengan lengkap

### Step 2: Install Dependencies (First Time Only)
```bash
# Login ke VPS
ssh root@13.212.217.150

# Install Docker
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh

# Install Docker Compose
sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose
```
- [ ] Docker installed (verify: `docker --version`)
- [ ] Docker Compose installed (verify: `docker-compose --version`)

### Step 3: Configure Firewall
```bash
sudo ufw allow 8080/tcp
sudo ufw enable
sudo ufw status
```
- [ ] Port 8080 allowed
- [ ] Firewall active

### Step 4: Deploy Application
```bash
cd /opt/monopoly-backend
chmod +x deploy.sh
./deploy.sh
```
- [ ] Build completed successfully
- [ ] Containers started without error
- [ ] No error in logs

### Step 5: Verify Deployment
```bash
# Check containers
docker-compose ps

# Check logs
docker-compose logs -f

# Test health endpoint
curl http://localhost:8080/health
```
- [ ] Both containers running (backend-api, nginx)
- [ ] Health check returns 200 OK
- [ ] No errors in logs

### Step 6: External Testing
```bash
# From local machine
curl http://13.212.217.150:8080/health

# From browser
http://13.212.217.150:8080/swagger
```
- [ ] API accessible from internet
- [ ] Swagger UI loads correctly
- [ ] Endpoints responding

## 🔧 Post-Deployment

### Monitoring
- [ ] Setup log monitoring: `docker-compose logs -f`
- [ ] Check resource usage: `docker stats`
- [ ] Verify auto-restart: `docker inspect monopoly-backend | grep -A 5 RestartPolicy`

### Documentation
- [ ] Update frontend API URL to: `http://13.212.217.150:8080`
- [ ] Document any custom configurations
- [ ] Share API documentation with team

### Optional Enhancements
- [ ] Setup domain name (if available)
- [ ] Configure SSL/HTTPS with Let's Encrypt
- [ ] Setup automated backups
- [ ] Configure monitoring/alerting

## 🚨 Troubleshooting

If deployment fails, check:
1. Docker logs: `docker-compose logs backend-api`
2. Port conflicts: `sudo netstat -tulpn | grep 8080`
3. Disk space: `df -h`
4. Memory: `free -h`
5. Firewall: `sudo ufw status`

## 📞 Quick Commands

```bash
# View logs
docker-compose logs -f

# Restart services
docker-compose restart

# Stop services
docker-compose down

# Rebuild and start
docker-compose build && docker-compose up -d

# Check status
docker-compose ps
```

## ✨ Success Criteria

Deployment is successful when:
- ✅ Containers are running
- ✅ API responds at http://13.212.217.150:8080
- ✅ Swagger UI accessible
- ✅ Health check returns 200 OK
- ✅ No errors in logs
- ✅ Frontend can connect to API

---

**Good luck with your deployment! 🚀**
