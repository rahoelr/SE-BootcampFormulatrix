#!/bin/bash

# Monopoly Backend Deployment Script
# This script automates the deployment process

set -e  # Exit on error

echo "========================================"
echo "Monopoly Backend Deployment Script"
echo "========================================"
echo ""

# Colors for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# Function to print colored messages
print_success() {
    echo -e "${GREEN}✓ $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠ $1${NC}"
}

print_error() {
    echo -e "${RED}✗ $1${NC}"
}

# Check if Docker is installed
if ! command -v docker &> /dev/null; then
    print_error "Docker is not installed. Please install Docker first."
    exit 1
fi
print_success "Docker is installed"

# Check if Docker Compose is installed
if ! command -v docker-compose &> /dev/null && ! docker compose version &> /dev/null; then
    print_error "Docker Compose is not installed. Please install Docker Compose first."
    exit 1
fi
print_success "Docker Compose is installed"

# Check if running as root or with sudo (for Linux)
if [[ "$OSTYPE" == "linux-gnu"* ]]; then
    if [ "$EUID" -ne 0 ] && ! groups | grep -q docker; then
        print_warning "You may need to run this script with sudo or add your user to the docker group"
        print_warning "To add user to docker group: sudo usermod -aG docker \$USER"
    fi
fi

echo ""
echo "Starting deployment process..."
echo ""

# Stop and remove existing containers
print_warning "Stopping existing containers..."
docker-compose down || docker compose down || true
print_success "Existing containers stopped"

# Remove old images (optional - commented out to save bandwidth)
# print_warning "Removing old images..."
# docker-compose down --rmi all || docker compose down --rmi all || true

# Build the Docker image
echo ""
print_warning "Building Docker image..."
if docker-compose build || docker compose build; then
    print_success "Docker image built successfully"
else
    print_error "Failed to build Docker image"
    exit 1
fi

# Start the containers
echo ""
print_warning "Starting containers..."
if docker-compose up -d || docker compose up -d; then
    print_success "Containers started successfully"
else
    print_error "Failed to start containers"
    exit 1
fi

# Wait for services to be ready
echo ""
print_warning "Waiting for services to be ready..."
sleep 10

# Check container status
echo ""
print_warning "Checking container status..."
docker-compose ps || docker compose ps

# Test the API
echo ""
print_warning "Testing API endpoint..."
if curl -f http://localhost:8080 > /dev/null 2>&1; then
    print_success "API is responding!"
else
    print_warning "API might not be ready yet. Check logs with: docker-compose logs -f"
fi

# Display logs
echo ""
print_warning "Recent logs:"
docker-compose logs --tail=20 || docker compose logs --tail=20

echo ""
echo "========================================"
print_success "Deployment completed!"
echo "========================================"
echo ""
echo "Access your API at:"
echo "  - http://localhost:8080"
echo "  - http://YOUR_VPS_IP:8080"
echo ""
echo "Swagger UI available at:"
echo "  - http://localhost:8080/swagger"
echo "  - http://YOUR_VPS_IP:8080/swagger"
echo ""
echo "Useful commands:"
echo "  - View logs: docker-compose logs -f"
echo "  - Stop services: docker-compose down"
echo "  - Restart services: docker-compose restart"
echo "  - View status: docker-compose ps"
echo ""
