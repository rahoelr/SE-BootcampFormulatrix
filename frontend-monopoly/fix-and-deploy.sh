#!/bin/bash

# ============================================
# PLAN A: Quick Fix Deployment Script
# ============================================
# This script fixes BASE_URL and redeploys
# Run this on VPS at /opt/SE-BootcampFormulatrix/frontend-monopoly
# ============================================

set -e  # Exit on error

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

# ============================================
# Step 1: Pre-Verification
# ============================================
echo -e "${YELLOW}→ Step 1: Verifying backend API...${NC}"

# Test backend locally
if curl -s http://localhost:8080/api/game/status > /dev/null 2>&1; then
    echo -e "${GREEN}✓ Backend responding on localhost:8080${NC}"
else
    echo -e "${RED}✗ Backend not responding on localhost:8080${NC}"
    echo -e "${YELLOW}  Checking backend container...${NC}"
    docker ps | grep monopoly-backend || echo -e "${RED}  Backend container not running!${NC}"
    exit 1
fi

# Test backend externally
if curl -s http://13.212.217.150:8080/api/game/status > /dev/null 2>&1; then
    echo -e "${GREEN}✓ Backend accessible externally${NC}"
else
    echo -e "${YELLOW}⚠ Backend not accessible externally (might be firewall)${NC}"
fi

echo ""

# ============================================
# Step 2: Fix BASE_URL in api.ts
# ============================================
echo -e "${YELLOW}→ Step 2: Fixing BASE_URL in api.ts...${NC}"

# Backup original file
cp src/services/api.ts src/services/api.ts.backup
echo -e "${GREEN}✓ Backup created: api.ts.backup${NC}"

# Show current configuration
echo -e "${YELLOW}Current configuration:${NC}"
grep "BASE_URL" src/services/api.ts | head -2

# Fix BASE_URL
sed -i "14s|^const BASE_URL = 'http://localhost:5278/api/game';|// const BASE_URL = 'http://localhost:5278/api/game';  // Development only|" src/services/api.ts 2>/dev/null || \
sed -i '' "14s|^const BASE_URL = 'http://localhost:5278/api/game';|// const BASE_URL = 'http://localhost:5278/api/game';  // Development only|" src/services/api.ts

sed -i "15s|^// const BASE_URL = 'http://13.212.217.150:8080/api/game';|const BASE_URL = 'http://13.212.217.150:8080/api/game';|" src/services/api.ts 2>/dev/null || \
sed -i '' "15s|^// const BASE_URL = 'http://13.212.217.150:8080/api/game';|const BASE_URL = 'http://13.212.217.150:8080/api/game';|" src/services/api.ts

echo -e "${YELLOW}New configuration:${NC}"
grep "BASE_URL" src/services/api.ts | head -2

# Verify changes
if grep -q "const BASE_URL = 'http://13.212.217.150:8080/api/game';" src/services/api.ts; then
    echo -e "${GREEN}✓ BASE_URL successfully updated to production${NC}"
else
    echo -e "${RED}✗ Failed to update BASE_URL${NC}"
    echo -e "${YELLOW}Restoring backup...${NC}"
    cp src/services/api.ts.backup src/services/api.ts
    exit 1
fi

echo ""

# ============================================
# Step 3: Build Docker Image
# ============================================
echo -e "${YELLOW}→ Step 3: Building Docker image...${NC}"
echo -e "${YELLOW}This may take 1-2 minutes...${NC}"

docker build -t monopoly-frontend . 

if [ $? -eq 0 ]; then
    echo -e "${GREEN}✓ Docker image built successfully${NC}"
else
    echo -e "${RED}✗ Docker build failed${NC}"
    echo -e "${YELLOW}Restoring backup...${NC}"
    cp src/services/api.ts.backup src/services/api.ts
    exit 1
fi

echo ""

# ============================================
# Step 4: Stop Old Container
# ============================================
echo -e "${YELLOW}→ Step 4: Stopping old container...${NC}"

if [ "$(docker ps -q -f name=monopoly-frontend)" ]; then
    docker stop monopoly-frontend
    echo -e "${GREEN}✓ Old container stopped${NC}"
fi

if [ "$(docker ps -aq -f name=monopoly-frontend)" ]; then
    docker rm monopoly-frontend
    echo -e "${GREEN}✓ Old container removed${NC}"
fi

echo ""

# ============================================
# Step 5: Deploy New Container
# ============================================
echo -e "${YELLOW}→ Step 5: Deploying new container on port 4000...${NC}"

docker run -d \
    --name monopoly-frontend \
    -p 4000:80 \
    --restart always \
    monopoly-frontend

if [ $? -eq 0 ]; then
    CONTAINER_ID=$(docker ps -q -f name=monopoly-frontend)
    echo -e "${GREEN}✓ Container deployed successfully${NC}"
    echo -e "${GREEN}  Container ID: ${CONTAINER_ID}${NC}"
else
    echo -e "${RED}✗ Failed to deploy container${NC}"
    exit 1
fi

echo ""

# ============================================
# Step 6: Health Check
# ============================================
echo -e "${YELLOW}→ Step 6: Running health checks...${NC}"
echo -e "${YELLOW}Waiting 5 seconds for container to start...${NC}"
sleep 5

# Check container is running
if [ "$(docker ps -q -f name=monopoly-frontend)" ]; then
    echo -e "${GREEN}✓ Container is running${NC}"
else
    echo -e "${RED}✗ Container stopped unexpectedly${NC}"
    echo -e "${YELLOW}Container logs:${NC}"
    docker logs monopoly-frontend
    exit 1
fi

# Test HTTP endpoint
HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:4000/ || echo "000")
if [ "$HTTP_CODE" = "200" ]; then
    echo -e "${GREEN}✓ HTTP endpoint responding (Status: $HTTP_CODE)${NC}"
else
    echo -e "${YELLOW}⚠ HTTP endpoint status: $HTTP_CODE${NC}"
    echo -e "${YELLOW}Container may need more time to start${NC}"
fi

# Test health endpoint
HEALTH=$(curl -s http://localhost:4000/health || echo "failed")
if [ "$HEALTH" = "healthy" ]; then
    echo -e "${GREEN}✓ Health check passed${NC}"
else
    echo -e "${YELLOW}⚠ Health check: $HEALTH${NC}"
fi

echo ""

# ============================================
# Step 7: Show Status
# ============================================
echo -e "${YELLOW}→ Step 7: Deployment status${NC}"
echo ""
echo "Container Status:"
docker ps -f name=monopoly-frontend --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
echo ""

# ============================================
# Step 8: Show Logs
# ============================================
echo -e "${YELLOW}→ Container logs (last 10 lines):${NC}"
docker logs --tail 10 monopoly-frontend
echo ""

# ============================================
# Step 9: Cleanup
# ============================================
echo -e "${YELLOW}→ Step 9: Cleanup...${NC}"
docker image prune -f > /dev/null 2>&1
echo -e "${GREEN}✓ Cleanup completed${NC}"
echo ""

# ============================================
# Success Message
# ============================================
echo -e "${BLUE}========================================${NC}"
echo -e "${GREEN}✓ Deployment Complete! 🚀${NC}"
echo -e "${BLUE}========================================${NC}"
echo ""
echo -e "${GREEN}Frontend URL:${NC}"
echo -e "  ${BLUE}http://13.212.217.150:4000${NC}"
echo ""
echo -e "${GREEN}Backend API:${NC}"
echo -e "  ${BLUE}http://13.212.217.150:8080/api/game${NC}"
echo ""
echo -e "${YELLOW}Next Steps:${NC}"
echo "  1. Open browser: http://13.212.217.150:4000"
echo "  2. Open DevTools (F12) → Network tab"
echo "  3. Verify API calls go to 13.212.217.150:8080"
echo "  4. Test game functionality"
echo ""
echo -e "${YELLOW}Useful Commands:${NC}"
echo "  docker logs -f monopoly-frontend      # View logs"
echo "  docker restart monopoly-frontend      # Restart"
echo "  docker stop monopoly-frontend         # Stop"
echo ""
echo -e "${YELLOW}Rollback (if needed):${NC}"
echo "  cp src/services/api.ts.backup src/services/api.ts"
echo "  docker build -t monopoly-frontend ."
echo "  docker stop monopoly-frontend && docker rm monopoly-frontend"
echo "  docker run -d --name monopoly-frontend -p 4000:80 --restart always monopoly-frontend"
echo ""
