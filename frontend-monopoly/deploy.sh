#!/bin/bash

# ====================================================
# Monopoly Frontend Deployment Script
# ====================================================
# This script automates the deployment of the React
# frontend to VPS using Docker + Nginx
# ====================================================

set -e  # Exit on error

# ====================================================
# CONFIGURATION
# ====================================================
VPS_HOST="vps-rahul"  # SSH config alias
VPS_USER="rahul"
VPS_IP="13.212.217.150"
VPS_PATH="/opt/SE-BootcampFormulatrix/frontend-monopoly"
CONTAINER_NAME="monopoly-frontend"
IMAGE_NAME="monopoly-frontend"
HOST_PORT="3000"
CONTAINER_PORT="80"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# ====================================================
# HELPER FUNCTIONS
# ====================================================

print_header() {
    echo -e "${BLUE}========================================${NC}"
    echo -e "${BLUE}$1${NC}"
    echo -e "${BLUE}========================================${NC}"
}

print_success() {
    echo -e "${GREEN}✓ $1${NC}"
}

print_error() {
    echo -e "${RED}✗ $1${NC}"
}

print_info() {
    echo -e "${YELLOW}→ $1${NC}"
}

# ====================================================
# DEPLOYMENT STEPS
# ====================================================

step_sync_files() {
    print_header "Step 1: Syncing Files to VPS"
    
    print_info "Syncing local files to $VPS_HOST:$VPS_PATH..."
    
    rsync -avz --delete \
        --exclude 'node_modules' \
        --exclude 'dist' \
        --exclude '.git' \
        --exclude '.env*' \
        --exclude '*.log' \
        -e "ssh -i ~/Downloads/aws-key-rhl.pem" \
        ./ ${VPS_USER}@${VPS_IP}:${VPS_PATH}/
    
    if [ $? -eq 0 ]; then
        print_success "Files synced successfully"
    else
        print_error "Failed to sync files"
        exit 1
    fi
}

step_build_image() {
    print_header "Step 2: Building Docker Image on VPS"
    
    print_info "Building Docker image on VPS..."
    
    ssh -i ~/Downloads/aws-key-rhl.pem ${VPS_USER}@${VPS_IP} << 'ENDSSH'
cd /opt/SE-BootcampFormulatrix/frontend-monopoly
echo "→ Building Docker image..."
docker build -t monopoly-frontend . 2>&1 | tail -20
if [ $? -eq 0 ]; then
    echo "✓ Docker image built successfully"
else
    echo "✗ Failed to build Docker image"
    exit 1
fi
ENDSSH
    
    if [ $? -eq 0 ]; then
        print_success "Docker image built successfully"
    else
        print_error "Failed to build Docker image"
        exit 1
    fi
}

step_stop_old_container() {
    print_header "Step 3: Stopping Old Container"
    
    print_info "Stopping and removing old container (if exists)..."
    
    ssh -i ~/Downloads/aws-key-rhl.pem ${VPS_USER}@${VPS_IP} << 'ENDSSH'
if [ "$(docker ps -q -f name=monopoly-frontend)" ]; then
    echo "→ Stopping running container..."
    docker stop monopoly-frontend
fi

if [ "$(docker ps -aq -f name=monopoly-frontend)" ]; then
    echo "→ Removing old container..."
    docker rm monopoly-frontend
fi
echo "✓ Old container cleaned up"
ENDSSH
    
    print_success "Old container removed (if existed)"
}

step_run_container() {
    print_header "Step 4: Running New Container"
    
    print_info "Starting new container on port $HOST_PORT..."
    
    ssh -i ~/Downloads/aws-key-rhl.pem ${VPS_USER}@${VPS_IP} << ENDSSH
docker run -d \
    --name ${CONTAINER_NAME} \
    -p ${HOST_PORT}:${CONTAINER_PORT} \
    --restart always \
    ${IMAGE_NAME}

if [ \$? -eq 0 ]; then
    echo "✓ Container started successfully"
    echo "→ Container ID: \$(docker ps -q -f name=${CONTAINER_NAME})"
else
    echo "✗ Failed to start container"
    exit 1
fi
ENDSSH
    
    if [ $? -eq 0 ]; then
        print_success "Container running on port $HOST_PORT"
    else
        print_error "Failed to start container"
        exit 1
    fi
}

step_health_check() {
    print_header "Step 5: Health Check"
    
    print_info "Waiting for container to be healthy..."
    sleep 5
    
    ssh -i ~/Downloads/aws-key-rhl.pem ${VPS_USER}@${VPS_IP} << 'ENDSSH'
# Check if container is running
if [ "$(docker ps -q -f name=monopoly-frontend)" ]; then
    echo "✓ Container is running"
    
    # Check container health
    HEALTH=$(docker inspect --format='{{.State.Health.Status}}' monopoly-frontend 2>/dev/null || echo "no-healthcheck")
    if [ "$HEALTH" != "unhealthy" ]; then
        echo "✓ Container health: $HEALTH"
    else
        echo "✗ Container is unhealthy"
        exit 1
    fi
    
    # Test HTTP endpoint
    HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:3000/ || echo "000")
    if [ "$HTTP_CODE" = "200" ]; then
        echo "✓ HTTP endpoint responding (Status: $HTTP_CODE)"
    else
        echo "⚠ HTTP endpoint status: $HTTP_CODE"
    fi
else
    echo "✗ Container is not running"
    exit 1
fi
ENDSSH
    
    if [ $? -eq 0 ]; then
        print_success "Health check passed"
    else
        print_error "Health check failed"
        exit 1
    fi
}

step_cleanup() {
    print_header "Step 6: Cleanup"
    
    print_info "Removing unused Docker images..."
    
    ssh -i ~/Downloads/aws-key-rhl.pem ${VPS_USER}@${VPS_IP} << 'ENDSSH'
docker image prune -f > /dev/null 2>&1
echo "✓ Cleanup completed"
ENDSSH
    
    print_success "Cleanup completed"
}

step_show_logs() {
    print_header "Container Logs (Last 20 lines)"
    
    ssh -i ~/Downloads/aws-key-rhl.pem ${VPS_USER}@${VPS_IP} << 'ENDSSH'
docker logs --tail 20 monopoly-frontend
ENDSSH
}

step_show_status() {
    print_header "Deployment Status"
    
    ssh -i ~/Downloads/aws-key-rhl.pem ${VPS_USER}@${VPS_IP} << 'ENDSSH'
echo "Container Status:"
docker ps -f name=monopoly-frontend --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"

echo ""
echo "Docker Images:"
docker images monopoly-frontend --format "table {{.Repository}}\t{{.Tag}}\t{{.Size}}\t{{.CreatedAt}}"
ENDSSH
}

# ====================================================
# MAIN EXECUTION
# ====================================================

main() {
    print_header "Monopoly Frontend Deployment"
    echo ""
    echo "VPS: $VPS_IP"
    echo "Target Path: $VPS_PATH"
    echo "Container: $CONTAINER_NAME"
    echo "Port: $HOST_PORT"
    echo ""
    
    # Check if SSH config exists
    if ! grep -q "Host vps-rahul" ~/.ssh/config 2>/dev/null && ! grep -q "Host vps-rahul" ~/.colima/ssh_config 2>/dev/null; then
        print_error "SSH config 'vps-rahul' not found"
        print_info "Please ensure SSH config is set up correctly"
        exit 1
    fi
    
    # Execute deployment steps
    step_sync_files
    echo ""
    
    step_build_image
    echo ""
    
    step_stop_old_container
    echo ""
    
    step_run_container
    echo ""
    
    step_health_check
    echo ""
    
    step_cleanup
    echo ""
    
    step_show_logs
    echo ""
    
    step_show_status
    echo ""
    
    # Success message
    print_header "Deployment Complete! 🚀"
    echo ""
    print_success "Frontend is now running at:"
    echo -e "${GREEN}   http://${VPS_IP}:${HOST_PORT}${NC}"
    echo ""
    print_info "Useful commands:"
    echo "   View logs:      ssh $VPS_HOST 'docker logs -f $CONTAINER_NAME'"
    echo "   Restart:        ssh $VPS_HOST 'docker restart $CONTAINER_NAME'"
    echo "   Stop:           ssh $VPS_HOST 'docker stop $CONTAINER_NAME'"
    echo "   SSH to VPS:     ssh $VPS_HOST"
    echo ""
}

# Handle script arguments
case "${1:-}" in
    --logs)
        step_show_logs
        ;;
    --status)
        step_show_status
        ;;
    --build-only)
        step_sync_files
        step_build_image
        ;;
    --help)
        echo "Usage: $0 [OPTIONS]"
        echo ""
        echo "Options:"
        echo "  (no args)      Full deployment"
        echo "  --logs         Show container logs"
        echo "  --status       Show deployment status"
        echo "  --build-only   Only sync and build, don't deploy"
        echo "  --help         Show this help"
        ;;
    *)
        main
        ;;
esac
