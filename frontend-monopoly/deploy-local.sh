#!/bin/bash

# ====================================================
# Monopoly Frontend Local Deployment Script
# ====================================================
# This script deploys directly on VPS (no SSH needed)
# ====================================================

set -e  # Exit on error

# ====================================================
# CONFIGURATION
# ====================================================
CONTAINER_NAME="monopoly-frontend"
IMAGE_NAME="monopoly-frontend"
HOST_PORT="4000"
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

step_check_docker() {
    print_header "Step 1: Checking Docker"
    
    if ! command -v docker &> /dev/null; then
        print_error "Docker is not installed"
        exit 1
    fi
    
    print_success "Docker is installed: $(docker --version)"
    
    if ! docker ps &> /dev/null; then
        print_error "Docker daemon is not running or permission denied"
        print_info "Try: sudo usermod -aG docker $USER && newgrp docker"
        exit 1
    fi
    
    print_success "Docker daemon is running"
}

step_build_image() {
    print_header "Step 2: Building Docker Image"
    
    print_info "Building Docker image..."
    
    docker build -t ${IMAGE_NAME} .
    
    if [ $? -eq 0 ]; then
        print_success "Docker image built successfully"
    else
        print_error "Failed to build Docker image"
        exit 1
    fi
}

step_stop_old_container() {
    print_header "Step 3: Stopping Old Container"
    
    if [ "$(docker ps -q -f name=${CONTAINER_NAME})" ]; then
        print_info "Stopping running container..."
        docker stop ${CONTAINER_NAME}
        print_success "Container stopped"
    else
        print_info "No running container found"
    fi

    if [ "$(docker ps -aq -f name=${CONTAINER_NAME})" ]; then
        print_info "Removing old container..."
        docker rm ${CONTAINER_NAME}
        print_success "Container removed"
    fi
}

step_run_container() {
    print_header "Step 4: Running New Container"
    
    print_info "Starting container on port ${HOST_PORT}..."
    
    docker run -d \
        --name ${CONTAINER_NAME} \
        -p ${HOST_PORT}:${CONTAINER_PORT} \
        --restart always \
        ${IMAGE_NAME}
    
    if [ $? -eq 0 ]; then
        print_success "Container started successfully"
        print_info "Container ID: $(docker ps -q -f name=${CONTAINER_NAME})"
    else
        print_error "Failed to start container"
        exit 1
    fi
}

step_health_check() {
    print_header "Step 5: Health Check"
    
    print_info "Waiting for container to be healthy..."
    sleep 5
    
    # Check if container is running
    if [ "$(docker ps -q -f name=${CONTAINER_NAME})" ]; then
        print_success "Container is running"
        
        # Check container health
        HEALTH=$(docker inspect --format='{{.State.Health.Status}}' ${CONTAINER_NAME} 2>/dev/null || echo "no-healthcheck")
        if [ "$HEALTH" != "unhealthy" ]; then
            print_success "Container health: $HEALTH"
        else
            print_error "Container is unhealthy"
            exit 1
        fi
        
        # Test HTTP endpoint
        HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:${HOST_PORT}/ || echo "000")
        if [ "$HTTP_CODE" = "200" ]; then
            print_success "HTTP endpoint responding (Status: $HTTP_CODE)"
        else
            print_info "HTTP endpoint status: $HTTP_CODE (might need more time)"
        fi
    else
        print_error "Container is not running"
        exit 1
    fi
}

step_cleanup() {
    print_header "Step 6: Cleanup"
    
    print_info "Removing unused Docker images..."
    docker image prune -f > /dev/null 2>&1
    print_success "Cleanup completed"
}

step_show_logs() {
    print_header "Container Logs (Last 20 lines)"
    docker logs --tail 20 ${CONTAINER_NAME}
}

step_show_status() {
    print_header "Deployment Status"
    
    echo "Container Status:"
    docker ps -f name=${CONTAINER_NAME} --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
    
    echo ""
    echo "Docker Images:"
    docker images ${IMAGE_NAME} --format "table {{.Repository}}\t{{.Tag}}\t{{.Size}}\t{{.CreatedAt}}"
}

# ====================================================
# MAIN EXECUTION
# ====================================================

main() {
    print_header "Monopoly Frontend Deployment"
    echo ""
    echo "Container: ${CONTAINER_NAME}"
    echo "Port: ${HOST_PORT}"
    echo "Working Directory: $(pwd)"
    echo ""
    
    # Execute deployment steps
    step_check_docker
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
    echo -e "${GREEN}   http://13.212.217.150:${HOST_PORT}${NC}"
    echo -e "${GREEN}   http://localhost:${HOST_PORT}${NC}"
    echo ""
    print_info "Useful commands:"
    echo "   View logs:      docker logs -f ${CONTAINER_NAME}"
    echo "   Restart:        docker restart ${CONTAINER_NAME}"
    echo "   Stop:           docker stop ${CONTAINER_NAME}"
    echo "   Remove:         docker rm -f ${CONTAINER_NAME}"
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
    --stop)
        print_info "Stopping container..."
        docker stop ${CONTAINER_NAME}
        print_success "Container stopped"
        ;;
    --restart)
        print_info "Restarting container..."
        docker restart ${CONTAINER_NAME}
        print_success "Container restarted"
        ;;
    --help)
        echo "Usage: $0 [OPTIONS]"
        echo ""
        echo "Options:"
        echo "  (no args)      Full deployment"
        echo "  --logs         Show container logs"
        echo "  --status       Show deployment status"
        echo "  --stop         Stop container"
        echo "  --restart      Restart container"
        echo "  --help         Show this help"
        ;;
    *)
        main
        ;;
esac
