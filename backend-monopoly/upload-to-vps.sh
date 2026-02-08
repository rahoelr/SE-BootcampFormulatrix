#!/bin/bash

# Upload Script untuk Deploy ke VPS
# Script ini akan mengupload project ke VPS Anda

set -e

# VPS Configuration
VPS_IP="13.212.217.150"
VPS_USER="root"
VPS_PATH="/opt/monopoly-backend"

# Colors
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
BLUE='\033[0;34m'
NC='\033[0m'

print_success() {
    echo -e "${GREEN}✓ $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠ $1${NC}"
}

print_error() {
    echo -e "${RED}✗ $1${NC}"
}

print_info() {
    echo -e "${BLUE}ℹ $1${NC}"
}

echo "================================================"
echo "  Upload Monopoly Backend ke VPS"
echo "================================================"
echo ""
print_info "VPS IP: ${VPS_IP}"
print_info "User: ${VPS_USER}"
print_info "Target Path: ${VPS_PATH}"
echo ""

# Confirm before proceeding
read -p "Apakah Anda ingin melanjutkan upload ke VPS? (y/n) " -n 1 -r
echo
if [[ ! $REPLY =~ ^[Yy]$ ]]
then
    print_warning "Upload dibatalkan"
    exit 1
fi

# Check if SCP is available
if ! command -v scp &> /dev/null; then
    print_error "SCP tidak ditemukan. Install OpenSSH client terlebih dahulu."
    exit 1
fi

print_info "Memulai upload..."
echo ""

# Create directory on VPS
print_warning "Membuat directory di VPS..."
ssh ${VPS_USER}@${VPS_IP} "mkdir -p ${VPS_PATH}" || {
    print_error "Gagal membuat directory di VPS"
    exit 1
}
print_success "Directory berhasil dibuat"

# Upload project files
print_warning "Uploading project files..."
scp -r \
    Dockerfile \
    docker-compose.yml \
    deploy.sh \
    .dockerignore \
    appsettings.Production.json \
    appsettings.json \
    appsettings.Development.json \
    backend-monopoly.csproj \
    backend-monopoly.sln \
    Program.cs \
    nginx/ \
    Controllers/ \
    Services/ \
    Models/ \
    DTOs/ \
    Enums/ \
    Interfaces/ \
    Common/ \
    Structs/ \
    Properties/ \
    ${VPS_USER}@${VPS_IP}:${VPS_PATH}/ || {
    print_error "Gagal upload files"
    exit 1
}
print_success "Files berhasil diupload"

# Make deploy.sh executable on VPS
print_warning "Setting permissions..."
ssh ${VPS_USER}@${VPS_IP} "chmod +x ${VPS_PATH}/deploy.sh" || {
    print_error "Gagal setting permissions"
    exit 1
}
print_success "Permissions berhasil diset"

echo ""
echo "================================================"
print_success "Upload selesai!"
echo "================================================"
echo ""
print_info "Langkah selanjutnya:"
echo "1. SSH ke VPS:"
echo "   ssh ${VPS_USER}@${VPS_IP}"
echo ""
echo "2. Masuk ke directory project:"
echo "   cd ${VPS_PATH}"
echo ""
echo "3. Jalankan deployment:"
echo "   ./deploy.sh"
echo ""
echo "4. Test API:"
echo "   curl http://localhost:8080/health"
echo ""
echo "5. Akses dari browser:"
echo "   http://${VPS_IP}:8080/swagger"
echo ""
print_success "Selamat! Project siap untuk di-deploy!"
echo ""
