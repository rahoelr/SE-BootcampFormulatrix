# 🚀 PLAN A - Quick Fix Deployment Guide

## ✅ What Has Been Done

1. ✅ **Fixed BASE_URL** in `src/services/api.ts`
   - Changed from: `http://localhost:5278/api/game`
   - Changed to: `http://13.212.217.150:8080/api/game`

2. ✅ **Created deployment script**: `fix-and-deploy.sh`
   - Automated complete deployment process
   - Includes verification and health checks
   - Has rollback capability

## 🎯 What You Need to Do on VPS

### Option 1: Using Automated Script (RECOMMENDED)

**Step 1: SSH to VPS**
```bash
ssh -i ~/Downloads/aws-key-rhl.pem rahul@13.212.217.150
```

**Step 2: Navigate to project directory**
```bash
cd /opt/SE-BootcampFormulatrix/frontend-monopoly
```

**Step 3: Create and run the deployment script**

Copy-paste this complete command:

```bash
cat > fix-and-deploy.sh << 'EOFSCRIPT'
#!/bin/bash

# ============================================
# PLAN A: Quick Fix Deployment Script
# ============================================

set -e

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

echo -e "${BLUE}========================================${NC}"
echo -e "${BLUE}Plan A: Quick Fix Deployment${NC}"
echo -e "${BLUE}========================================${NC}"
echo ""

# Step 1: Pre-Verification
echo -e "${YELLOW}→ Step 1: Verifying backend API...${NC}"
if curl -s http://localhost:8080/api/game/status > /dev/null 2>&1; then
    echo -e "${GREEN}✓ Backend responding${NC}"
else
    echo -e "${RED}✗ Backend not responding${NC}"
    exit 1
fi
echo ""

# Step 2: Backup and fix BASE_URL
echo -e "${YELLOW}→ Step 2: Fixing BASE_URL...${NC}"
cp src/services/api.ts src/services/api.ts.backup
sed -i "s|const BASE_URL = 'http://localhost:5278/api/game';|// const BASE_URL = 'http://localhost:5278/api/game';  // Development only|" src/services/api.ts
sed -i "s|// const BASE_URL = 'http://13.212.217.150:8080/api/game';|const BASE_URL = 'http://13.212.217.150:8080/api/game';|" src/services/api.ts

if grep -q "const BASE_URL = 'http://13.212.217.150:8080/api/game';" src/services/api.ts; then
    echo -e "${GREEN}✓ BASE_URL updated${NC}"
else
    echo -e "${RED}✗ Failed to update BASE_URL${NC}"
    exit 1
fi
echo ""

# Step 3: Build Docker image
echo -e "${YELLOW}→ Step 3: Building Docker image (1-2 min)...${NC}"
docker build -t monopoly-frontend .
echo -e "${GREEN}✓ Build complete${NC}"
echo ""

# Step 4: Stop old container
echo -e "${YELLOW}→ Step 4: Stopping old container...${NC}"
docker stop monopoly-frontend 2>/dev/null || true
docker rm monopoly-frontend 2>/dev/null || true
echo -e "${GREEN}✓ Old container removed${NC}"
echo ""

# Step 5: Deploy new container
echo -e "${YELLOW}→ Step 5: Deploying on port 4000...${NC}"
docker run -d \
    --name monopoly-frontend \
    -p 4000:80 \
    --restart always \
    monopoly-frontend
echo -e "${GREEN}✓ Container deployed${NC}"
echo ""

# Step 6: Health check
echo -e "${YELLOW}→ Step 6: Health check...${NC}"
sleep 5
if [ "$(docker ps -q -f name=monopoly-frontend)" ]; then
    echo -e "${GREEN}✓ Container running${NC}"
    HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:4000/ || echo "000")
    echo -e "${GREEN}✓ HTTP Status: $HTTP_CODE${NC}"
else
    echo -e "${RED}✗ Container not running${NC}"
    exit 1
fi
echo ""

# Success
echo -e "${BLUE}========================================${NC}"
echo -e "${GREEN}✓ Deployment Complete! 🚀${NC}"
echo -e "${BLUE}========================================${NC}"
echo ""
echo -e "${GREEN}Frontend: http://13.212.217.150:4000${NC}"
echo -e "${GREEN}Backend:  http://13.212.217.150:8080/api/game${NC}"
echo ""
docker ps -f name=monopoly-frontend
echo ""

EOFSCRIPT

chmod +x fix-and-deploy.sh
./fix-and-deploy.sh
```

---

### Option 2: Manual Step-by-Step

If you prefer manual control:

```bash
# 1. Navigate to directory
cd /opt/SE-BootcampFormulatrix/frontend-monopoly

# 2. Verify backend
curl http://localhost:8080/api/game/status

# 3. Backup original file
cp src/services/api.ts src/services/api.ts.backup

# 4. Fix BASE_URL (choose one method)

# Method A: Using sed
sed -i "s|const BASE_URL = 'http://localhost:5278/api/game';|// const BASE_URL = 'http://localhost:5278/api/game';  // Development only|" src/services/api.ts
sed -i "s|// const BASE_URL = 'http://13.212.217.150:8080/api/game';|const BASE_URL = 'http://13.212.217.150:8080/api/game';|" src/services/api.ts

# Method B: Using nano
nano src/services/api.ts
# Edit lines 14-15 manually, Ctrl+O to save, Ctrl+X to exit

# 5. Verify changes
grep "BASE_URL" src/services/api.ts | head -2

# 6. Build Docker image
docker build -t monopoly-frontend .

# 7. Stop old container
docker stop monopoly-frontend 2>/dev/null || true
docker rm monopoly-frontend 2>/dev/null || true

# 8. Run new container
docker run -d \
    --name monopoly-frontend \
    -p 4000:80 \
    --restart always \
    monopoly-frontend

# 9. Verify deployment
docker ps | grep monopoly-frontend
docker logs monopoly-frontend
curl http://localhost:4000/
```

---

## ✅ Verification Checklist

After deployment, verify:

### 1. Container Status
```bash
docker ps -f name=monopoly-frontend
```
**Expected:** Container shows "Up X seconds"

### 2. HTTP Endpoint
```bash
curl -I http://localhost:4000/
```
**Expected:** `HTTP/1.1 200 OK`

### 3. Container Logs
```bash
docker logs monopoly-frontend
```
**Expected:** No error messages

### 4. Browser Test
Open: **http://13.212.217.150:4000**

**Expected:**
- ✅ Page loads
- ✅ No console errors
- ✅ API calls go to `13.212.217.150:8080`

### 5. DevTools Network Tab
Open F12 → Network tab, then interact with the app

**Expected:**
- ✅ Requests to: `http://13.212.217.150:8080/api/game/status`
- ✅ Requests to: `http://13.212.217.150:8080/api/game/board`
- ✅ Status: `200 OK` (not `ERR_CONNECTION_REFUSED`)

---

## 🔧 Troubleshooting

### If deployment fails:

**1. Backend not responding**
```bash
docker ps | grep monopoly-backend
docker restart monopoly-backend
docker logs monopoly-backend
```

**2. Build fails**
```bash
# Check syntax
cat src/services/api.ts | head -20

# Restore backup
cp src/services/api.ts.backup src/services/api.ts
```

**3. Container won't start**
```bash
docker logs monopoly-frontend
docker inspect monopoly-frontend
```

**4. Port already in use**
```bash
# Check what's using port 4000
sudo netstat -tuln | grep :4000

# Use different port
docker run -d --name monopoly-frontend -p 5001:80 --restart always monopoly-frontend
```

---

## 🔄 Rollback Instructions

If something goes wrong:

```bash
cd /opt/SE-BootcampFormulatrix/frontend-monopoly

# Restore original configuration
cp src/services/api.ts.backup src/services/api.ts

# Rebuild and redeploy
docker build -t monopoly-frontend .
docker stop monopoly-frontend && docker rm monopoly-frontend
docker run -d --name monopoly-frontend -p 4000:80 --restart always monopoly-frontend
```

---

## 📊 Expected Results

### Before Fix (Current State ❌)
- Frontend tries to connect to: `localhost:5278`
- Browser console shows: `ERR_CONNECTION_REFUSED`
- API calls fail with network errors

### After Fix (Expected State ✅)
- Frontend connects to: `13.212.217.150:8080`
- No connection errors
- API calls succeed
- Game fully functional

---

## 🎯 Summary

**Total Time:** ~3-5 minutes
**Risk Level:** LOW
**Rollback:** Available

**Files Modified:**
- `src/services/api.ts` (lines 14-15)

**Changes:**
- BASE_URL changed from localhost to production IP

**Deployment:**
- Container: `monopoly-frontend`
- Port: `4000`
- URL: `http://13.212.217.150:4000`

---

## 📞 Need Help?

If you encounter issues:

1. Check container logs: `docker logs monopoly-frontend`
2. Check backend status: `curl http://localhost:8080/api/game/status`
3. Verify file changes: `cat src/services/api.ts | head -20`
4. Use rollback if needed (see above)

---

**Ready to deploy? Copy the automated script command from Option 1 above and run it on your VPS!** 🚀
