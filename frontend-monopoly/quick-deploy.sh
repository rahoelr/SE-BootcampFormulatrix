#!/bin/bash

# ====================================================
# QUICK DEPLOYMENT GUIDE - Port 4000
# ====================================================
# Run this directly on VPS
# ====================================================

echo "========================================="
echo "Monopoly Frontend Deployment - Port 4000"
echo "========================================="
echo ""

# Step 1: Build Docker image
echo "→ Building Docker image..."
docker build -t monopoly-frontend .

if [ $? -ne 0 ]; then
    echo "✗ Build failed!"
    exit 1
fi
echo "✓ Build successful!"
echo ""

# Step 2: Stop old container if exists
echo "→ Stopping old container (if exists)..."
docker stop monopoly-frontend 2>/dev/null || true
docker rm monopoly-frontend 2>/dev/null || true
echo "✓ Old container removed"
echo ""

# Step 3: Run new container on port 4000
echo "→ Starting new container on port 4000..."
docker run -d \
    --name monopoly-frontend \
    -p 4000:80 \
    --restart always \
    monopoly-frontend

if [ $? -ne 0 ]; then
    echo "✗ Failed to start container!"
    exit 1
fi
echo "✓ Container started!"
echo ""

# Step 4: Wait and check
echo "→ Waiting for container to be ready..."
sleep 3

# Show status
echo "========================================="
echo "Deployment Status"
echo "========================================="
docker ps -f name=monopoly-frontend --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
echo ""

# Test endpoint
echo "→ Testing endpoint..."
HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:4000/ || echo "000")
if [ "$HTTP_CODE" = "200" ]; then
    echo "✓ Frontend is responding!"
else
    echo "⚠ HTTP Status: $HTTP_CODE (might need more time)"
fi
echo ""

echo "========================================="
echo "✓ Deployment Complete! 🚀"
echo "========================================="
echo ""
echo "Frontend URL: http://13.212.217.150:4000"
echo ""
echo "Useful commands:"
echo "  docker logs -f monopoly-frontend      # View logs"
echo "  docker restart monopoly-frontend      # Restart"
echo "  docker stop monopoly-frontend         # Stop"
echo ""
