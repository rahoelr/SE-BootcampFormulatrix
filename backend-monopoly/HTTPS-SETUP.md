# 🔒 HTTPS Setup Guide - Cloudflare Tunnel

## ⚠️ Masalah: Mixed Content Error

Jika frontend Anda di-hosting di HTTPS (seperti Vercel), browser akan memblokir request ke backend HTTP:

```
Mixed Content: The page at 'https://monopoly-formulatrix.vercel.app/' was loaded over HTTPS, 
but requested an insecure XMLHttpRequest endpoint 'http://13.212.217.150:8080/api/game/status'. 
This request has been blocked; the content must be served over HTTPS.
```

## ✅ Solusi: Cloudflare Tunnel

Cloudflare Tunnel memberikan **HTTPS gratis tanpa perlu domain atau SSL certificate**!

### Keuntungan:
- ✅ HTTPS URL gratis (contoh: `https://your-app.trycloudflare.com`)
- ✅ Tidak perlu beli domain
- ✅ Tidak perlu setup SSL certificate
- ✅ Tidak perlu open port di firewall
- ✅ Zero configuration SSL/TLS
- ✅ DDoS protection dari Cloudflare
- ✅ Akses dari mana saja dengan HTTPS

---

## 🚀 Setup Cloudflare Tunnel (15 menit)

### Step 1: Buat Akun Cloudflare (Gratis)

1. Buka: https://dash.cloudflare.com/sign-up
2. Daftar dengan email
3. Verifikasi email
4. Login ke dashboard

### Step 2: Buat Tunnel

1. Login ke Cloudflare dashboard: https://one.dash.cloudflare.com/
2. Pilih **"Zero Trust"** dari sidebar kiri
3. Jika diminta, klik **"Get Started"** dan buat team name (misal: `monopoly-team`)
4. Pilih **"Access"** → **"Tunnels"**
5. Klik **"Create a tunnel"**
6. Pilih **"Cloudflared"** sebagai connector
7. Beri nama tunnel (misal: `monopoly-backend`)
8. Klik **"Save tunnel"**

### Step 3: Setup Connector (Get Token)

1. Setelah tunnel dibuat, Anda akan melihat halaman "Install connector"
2. Pilih **"Docker"** sebagai environment
3. **COPY token** yang ditampilkan
   ```
   Contoh token: eyJhIjoiNzg5YWJjZGVmMTIzNDU2Nzg5MGFiY2RlZjEyMzQ1Njc4IiwidCI6IjEyMzQ1Njc4LWFiY2QtZWZnaC1pamsxLTIzNDU2Nzg5YWJjZCIsInMiOiJhYmNkZWZnaGlqa2xtbm9wcXJzdHV2d3h5ejEyMzQ1Njc4OTAifQ==
   ```
4. **SIMPAN token ini** - akan dibutuhkan untuk deployment

### Step 4: Setup Public Hostname

1. Pilih tab **"Public Hostname"**
2. Klik **"Add a public hostname"**
3. Isi konfigurasi:
   - **Subdomain**: `monopoly-api` (atau nama lain yang Anda inginkan)
   - **Domain**: Pilih yang auto-generated oleh Cloudflare (biasanya `*.trycloudflare.com`)
   - **Path**: (kosongkan)
   - **Service**:
     - Type: `HTTP`
     - URL: `nginx:80`
4. Klik **"Save hostname"**

5. **COPY URL HTTPS** yang digenerate (contoh: `https://monopoly-api.trycloudflare.com`)

### Step 5: Jalankan Setup Script

Di komputer lokal, jalankan:

```bash
./setup-cloudflare-tunnel.sh
```

Script akan memandu Anda dan membuat file `.env` dengan token.

**ATAU** buat manual:

```bash
# Buat file .env
cat > .env << 'EOF'
CLOUDFLARE_TUNNEL_TOKEN=<your_token_here>
EOF
```

Ganti `<your_token_here>` dengan token yang Anda copy dari Cloudflare.

### Step 6: Deploy ke VPS

```bash
# Upload ke VPS (termasuk .env file)
./upload-to-vps.sh

# SSH ke VPS
ssh root@13.212.217.150

# Deploy (akan otomatis start Cloudflare Tunnel)
cd /opt/monopoly-backend
./deploy.sh
```

### Step 7: Verify Deployment

1. **Check logs Cloudflare Tunnel:**
   ```bash
   docker-compose logs cloudflared
   ```

   Anda harus melihat:
   ```
   Connection <UUID> registered connIndex=0
   ```

2. **Test API via HTTPS:**
   ```bash
   curl https://monopoly-api.trycloudflare.com/health
   ```

3. **Buka Swagger di browser:**
   ```
   https://monopoly-api.trycloudflare.com/swagger
   ```

4. **Update Frontend:**
   Update API base URL di frontend Anda:
   ```javascript
   // Ubah dari:
   const API_URL = 'http://13.212.217.150:8080'
   
   // Ke:
   const API_URL = 'https://monopoly-api.trycloudflare.com'
   ```

---

## 📊 Architecture dengan Cloudflare Tunnel

```
Internet (HTTPS)
    ↓
Cloudflare Global Network
    ↓
Cloudflare Tunnel (cloudflared container)
    ↓
Nginx Container (Port 80)
    ↓
Backend API Container (Port 8080)
    ↓
.NET 10.0 Application
```

**Keuntungan:**
- ✅ End-to-end encryption (HTTPS)
- ✅ No exposed ports (lebih aman)
- ✅ DDoS protection
- ✅ Global CDN
- ✅ Auto SSL renewal
- ✅ Mixed content error solved

---

## 🔧 Management Commands

```bash
# View Cloudflare Tunnel logs
docker-compose logs -f cloudflared

# Restart tunnel
docker-compose restart cloudflared

# Check tunnel status
docker-compose ps

# Stop tunnel
docker-compose stop cloudflared

# Start tunnel
docker-compose start cloudflared
```

---

## 🆘 Troubleshooting

### Problem 1: Token error

**Error:**
```
ERR Error registering tunnel err="unable to find token"
```

**Solution:**
```bash
# Check if .env file exists
cat .env

# Verify token is set
echo $CLOUDFLARE_TUNNEL_TOKEN

# If empty, recreate .env file
./setup-cloudflare-tunnel.sh
```

### Problem 2: Connection refused

**Error:**
```
ERR  error="Unable to reach the origin service"
```

**Solution:**
```bash
# Check if nginx is running
docker-compose ps nginx

# Check nginx logs
docker-compose logs nginx

# Restart services
docker-compose restart nginx
docker-compose restart cloudflared
```

### Problem 3: Tunnel not connecting

**Solution:**
```bash
# Check cloudflared logs
docker-compose logs cloudflared

# Verify token is correct in Cloudflare dashboard
# Tunnels → Your Tunnel → Configure

# Recreate tunnel if needed
docker-compose down
docker-compose up -d
```

### Problem 4: 502 Bad Gateway

**Solution:**
```bash
# Check backend API is running
docker-compose ps backend-api

# Check backend logs
docker-compose logs backend-api

# Check health endpoint
curl http://localhost:8080/health

# Restart backend
docker-compose restart backend-api
```

---

## 📝 FAQ

### Q: Apakah Cloudflare Tunnel gratis?
**A:** Ya! Cloudflare Tunnel sepenuhnya gratis untuk penggunaan personal dan commercial.

### Q: Apakah perlu domain sendiri?
**A:** Tidak! Cloudflare memberikan subdomain gratis (*.trycloudflare.com).

### Q: Apakah bisa gunakan domain sendiri?
**A:** Ya! Anda bisa menggunakan domain sendiri dengan menambahkan domain ke Cloudflare dan setup DNS.

### Q: Apakah tunnel stabil?
**A:** Ya! Cloudflare Tunnel adalah production-ready dan digunakan oleh jutaan aplikasi.

### Q: Bagaimana cara update token?
**A:** Edit file `.env` di VPS, lalu restart: `docker-compose restart cloudflared`

### Q: Apakah bisa multiple tunnels?
**A:** Ya! Anda bisa membuat multiple tunnels untuk berbagai services.

### Q: Bagaimana cara monitoring?
**A:** Check di Cloudflare dashboard → Zero Trust → Access → Tunnels untuk melihat status dan traffic.

---

## 🎯 Next Steps

1. ✅ Setup Cloudflare Tunnel
2. ✅ Deploy ke VPS
3. ✅ Verify HTTPS working
4. ✅ Update frontend API URL
5. ✅ Test dari frontend
6. ✅ Monitor di Cloudflare dashboard

---

## 🔗 Useful Links

- Cloudflare Dashboard: https://dash.cloudflare.com/
- Cloudflare Zero Trust: https://one.dash.cloudflare.com/
- Cloudflare Tunnel Docs: https://developers.cloudflare.com/cloudflare-one/connections/connect-apps/
- Cloudflare Tunnel GitHub: https://github.com/cloudflare/cloudflared

---

**🎊 Selamat! Backend Anda sekarang accessible via HTTPS! 🎊**
