# 🚀 Deployment Guide - Monopoly Frontend

This document provides instructions for deploying the Monopoly React.js frontend to VPS using Docker and Nginx.

---

## 📋 Table of Contents

- [Overview](#overview)
- [Prerequisites](#prerequisites)
- [Quick Start](#quick-start)
- [Configuration](#configuration)
- [Deployment Steps](#deployment-steps)
- [Verification](#verification)
- [Troubleshooting](#troubleshooting)
- [Manual Deployment](#manual-deployment)
- [Useful Commands](#useful-commands)

---

## 🎯 Overview

**Deployment Architecture:**
- **VPS IP:** 13.212.217.150
- **Frontend URL:** http://13.212.217.150:3000
- **Backend API:** http://13.212.217.150:8080/api/game
- **Method:** Docker + Nginx
- **Build Tool:** Vite (React + TypeScript)
- **Web Server:** Nginx (Alpine Linux)

**Docker Configuration:**
- Multi-stage build (Node.js 20 → Nginx Alpine)
- Port mapping: `3000:80` (Host:Container)
- Auto-restart on failure
- Health checks enabled

---

## ✅ Prerequisites

### Local Machine
- [x] SSH access to VPS configured
- [x] SSH key: `~/Downloads/aws-key-rhl.pem`
- [x] SSH config alias: `vps-rahul`
- [x] rsync installed (for file sync)

### VPS (13.212.217.150)
- [x] Docker installed
- [x] Docker daemon running
- [x] Port 3000 open (firewall rules)
- [x] Sufficient disk space (~500MB)
- [x] Backend running on port 8080

### Verify Prerequisites

```bash
# Check SSH access
ssh vps-rahul "echo 'SSH OK'"

# Check Docker on VPS
ssh vps-rahul "docker --version"

# Check Docker daemon
ssh vps-rahul "docker ps"

# Check port availability
ssh vps-rahul "sudo netstat -tuln | grep :3000"
```

---

## 🚀 Quick Start

### Automated Deployment (Recommended)

```bash
# Run the deployment script
./deploy.sh
```

That's it! The script will:
1. ✅ Sync files to VPS
2. ✅ Build Docker image
3. ✅ Stop old container
4. ✅ Run new container
5. ✅ Perform health checks
6. ✅ Show deployment status

**Expected Output:**
```
========================================
Monopoly Frontend Deployment
========================================

VPS: 13.212.217.150
Target Path: /opt/SE-BootcampFormulatrix/frontend-monopoly
Container: monopoly-frontend
Port: 3000

========================================
Step 1: Syncing Files to VPS
========================================
→ Syncing local files to vps-rahul...
✓ Files synced successfully

...

========================================
Deployment Complete! 🚀
========================================

✓ Frontend is now running at:
   http://13.212.217.150:3000
```

### View Logs Only

```bash
./deploy.sh --logs
```

### Check Status

```bash
./deploy.sh --status
```

### Build Without Deploying

```bash
./deploy.sh --build-only
```

---

## ⚙️ Configuration

### Environment Variables

The base URL is hardcoded in `src/services/api.ts`:

```typescript
const BASE_URL = 'http://13.212.217.150:8080/api/game';
```

To change the API URL:
1. Edit `src/services/api.ts`
2. Rebuild and redeploy: `./deploy.sh`

### Docker Configuration

**Dockerfile** - Multi-stage build:
- **Stage 1:** Build React app with Node.js 20
- **Stage 2:** Serve with Nginx Alpine

**nginx.conf** - Features:
- SPA routing (fallback to index.html)
- Gzip compression
- Static file caching (1 year)
- Security headers
- Health check endpoint: `/health`

### SSH Configuration

Ensure your SSH config includes:

```bash
# ~/.ssh/config or ~/.colima/ssh_config
Host vps-rahul
    HostName 13.212.217.150
    User rahul
    IdentityFile ~/Downloads/aws-key-rhl.pem
```

---

## 📦 Deployment Steps

### Step-by-Step Manual Deployment

If you prefer manual control:

#### 1. Sync Files to VPS

```bash
rsync -avz --delete \
    --exclude 'node_modules' \
    --exclude 'dist' \
    --exclude '.git' \
    -e "ssh -i ~/Downloads/aws-key-rhl.pem" \
    ./ rahul@13.212.217.150:/opt/SE-BootcampFormulatrix/frontend-monopoly/
```

#### 2. SSH to VPS

```bash
ssh vps-rahul
cd /opt/SE-BootcampFormulatrix/frontend-monopoly
```

#### 3. Build Docker Image

```bash
docker build -t monopoly-frontend .
```

#### 4. Stop Old Container (if exists)

```bash
docker stop monopoly-frontend || true
docker rm monopoly-frontend || true
```

#### 5. Run New Container

```bash
docker run -d \
    --name monopoly-frontend \
    -p 3000:80 \
    --restart always \
    monopoly-frontend
```

#### 6. Verify Deployment

```bash
# Check container status
docker ps | grep monopoly-frontend

# Check logs
docker logs monopoly-frontend

# Test endpoint
curl http://localhost:3000/
```

---

## ✅ Verification

### 1. Check Container Status

```bash
ssh vps-rahul "docker ps -f name=monopoly-frontend"
```

Expected output:
```
CONTAINER ID   IMAGE               STATUS          PORTS
abc123def456   monopoly-frontend   Up 2 minutes    0.0.0.0:3000->80/tcp
```

### 2. Test HTTP Endpoint

```bash
curl -I http://13.212.217.150:3000/
```

Expected: `HTTP/1.1 200 OK`

### 3. Test Health Check

```bash
curl http://13.212.217.150:3000/health
```

Expected: `healthy`

### 4. Test in Browser

Open: **http://13.212.217.150:3000**

You should see the Monopoly game interface.

### 5. Test API Connection

1. Open browser DevTools (F12)
2. Go to Network tab
3. Interact with the game (e.g., create new game)
4. Verify requests to `http://13.212.217.150:8080/api/game/*`

---

## 🔧 Troubleshooting

### Container Not Starting

```bash
# Check container logs
ssh vps-rahul "docker logs monopoly-frontend"

# Check if port is already in use
ssh vps-rahul "sudo netstat -tuln | grep :3000"

# Check Docker daemon
ssh vps-rahul "sudo systemctl status docker"
```

### Build Failures

```bash
# Clean Docker cache
ssh vps-rahul "docker system prune -f"

# Remove old images
ssh vps-rahul "docker rmi monopoly-frontend || true"

# Rebuild with no cache
ssh vps-rahul "cd /opt/SE-BootcampFormulatrix/frontend-monopoly && docker build --no-cache -t monopoly-frontend ."
```

### Cannot Access Frontend

1. **Check firewall:**
   ```bash
   ssh vps-rahul "sudo ufw status"
   ssh vps-rahul "sudo ufw allow 3000/tcp"
   ```

2. **Check container networking:**
   ```bash
   ssh vps-rahul "docker inspect monopoly-frontend | grep IPAddress"
   ```

3. **Check nginx logs:**
   ```bash
   ssh vps-rahul "docker exec monopoly-frontend cat /var/log/nginx/error.log"
   ```

### API Connection Issues

1. **Verify backend is running:**
   ```bash
   curl http://13.212.217.150:8080/api/game/status
   ```

2. **Check CORS settings** on backend

3. **Verify BASE_URL** in `src/services/api.ts`

### SSH Connection Issues

```bash
# Test SSH connection
ssh -i ~/Downloads/aws-key-rhl.pem rahul@13.212.217.150 "echo 'SSH OK'"

# Check SSH config
cat ~/.ssh/config | grep -A 5 "vps-rahul"
```

---

## 🛠️ Useful Commands

### Container Management

```bash
# View real-time logs
ssh vps-rahul "docker logs -f monopoly-frontend"

# Restart container
ssh vps-rahul "docker restart monopoly-frontend"

# Stop container
ssh vps-rahul "docker stop monopoly-frontend"

# Start container
ssh vps-rahul "docker start monopoly-frontend"

# Remove container
ssh vps-rahul "docker rm -f monopoly-frontend"

# Shell into container
ssh vps-rahul "docker exec -it monopoly-frontend sh"
```

### Docker Image Management

```bash
# List images
ssh vps-rahul "docker images monopoly-frontend"

# Remove old images
ssh vps-rahul "docker image prune -f"

# Remove specific image
ssh vps-rahul "docker rmi monopoly-frontend"

# View image details
ssh vps-rahul "docker inspect monopoly-frontend"
```

### Monitoring

```bash
# Container stats (CPU, Memory)
ssh vps-rahul "docker stats monopoly-frontend --no-stream"

# Disk usage
ssh vps-rahul "docker system df"

# Container processes
ssh vps-rahul "docker top monopoly-frontend"
```

### Quick Redeploy

```bash
# Full redeploy
./deploy.sh

# Or manually
ssh vps-rahul "cd /opt/SE-BootcampFormulatrix/frontend-monopoly && \
    docker stop monopoly-frontend && \
    docker rm monopoly-frontend && \
    docker build -t monopoly-frontend . && \
    docker run -d --name monopoly-frontend -p 3000:80 --restart always monopoly-frontend"
```

---

## 📊 File Structure

```
frontend-monopoly/
├── Dockerfile              # Multi-stage build configuration
├── nginx.conf             # Nginx server configuration
├── .dockerignore          # Files to exclude from Docker context
├── deploy.sh              # Automated deployment script
├── README-DEPLOYMENT.md   # This file
├── package.json           # Node.js dependencies
├── vite.config.ts         # Vite build configuration
├── src/
│   ├── services/
│   │   └── api.ts        # API base URL configuration
│   └── ...
└── ...
```

---

## 🔒 Security Considerations

1. **SSH Keys:** Keep `aws-key-rhl.pem` secure (chmod 600)
2. **Firewall:** Only open necessary ports (3000, 8080, 22)
3. **HTTPS:** Consider adding SSL with Let's Encrypt (requires domain)
4. **Environment Variables:** Don't commit sensitive data
5. **Docker Security:** Run containers with least privileges

---

## 📝 Notes

- **Port 3000:** Frontend accessible via http://13.212.217.150:3000
- **Port 8080:** Backend API (already configured)
- **Auto-restart:** Container restarts automatically on failure
- **Health Checks:** Built-in health monitoring
- **Logs:** Persistent logs via Docker
- **Rebuild:** Changes require rebuild + redeploy

---

## 🆘 Support

If you encounter issues:

1. Check container logs: `./deploy.sh --logs`
2. Verify all prerequisites
3. Review troubleshooting section
4. Check Docker daemon status
5. Verify network connectivity

---

## 📚 Additional Resources

- [Docker Documentation](https://docs.docker.com/)
- [Nginx Documentation](https://nginx.org/en/docs/)
- [Vite Documentation](https://vitejs.dev/)
- [React Documentation](https://react.dev/)

---

**Last Updated:** February 2026  
**Maintained By:** Deployment Team
