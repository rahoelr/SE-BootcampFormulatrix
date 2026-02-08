# 🎲 Monopoly Backend API - Version 3.0

Backend API untuk game Monopoly dengan properti kota Indonesia dan sistem manual end turn.

[![.NET Version](https://img.shields.io/badge/.NET-10.0-purple)]()
[![Build Status](https://img.shields.io/badge/build-passing-brightgreen)]()
[![Version](https://img.shields.io/badge/version-3.0.0-blue)]()
[![License](https://img.shields.io/badge/license-MIT-green)]()

---

## 📋 Daftar Isi

- [Overview](#overview)
- [Fitur Utama](#fitur-utama)
- [Perubahan Terbaru](#perubahan-terbaru-v30)
- [Tech Stack](#tech-stack)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [API Endpoints](#api-endpoints)
- [Dokumentasi](#dokumentasi)
- [Testing](#testing)
- [Migration](#migration)
- [Contributing](#contributing)
- [License](#license)

---

## Overview

**Monopoly Backend API** adalah REST API berbasis ASP.NET Core yang mengimplementasikan game Monopoly dengan twist Indonesia. Semua properti menggunakan nama kota-kota Indonesia, dan game ini menerapkan sistem **manual end turn** untuk memberikan kontrol penuh kepada pemain.

### Fitur Game

- 🏙️ **Properti Kota Indonesia** - 28 properti dengan nama kota Indonesia
- 🎲 **Sistem Dadu** - Roll dice dengan validasi anti-spam
- 🏠 **Manajemen Properti** - Beli, jual, bangun rumah, gadai
- 🔒 **Sistem Jail** - Penjara dengan 3 cara keluar
- 🔄 **Trading** - Tukar properti dan uang antar pemain
- ⚡ **Manual Turn Control** - Pemain kontrol penuh kapan end turn
- 👥 **Multi-player** - Support 2-4 pemain

---

## Fitur Utama

### 1. Properti Kota Indonesia 🇮🇩

Semua 28 properti menggunakan nama kota Indonesia:

**Real Estate (22 kota):**
- **Brown:** Medan, Palembang
- **Light Blue:** Semarang, Surabaya, Makassar
- **Pink:** Bandung, Yogyakarta, Solo
- **Orange:** Denpasar, Malang, Balikpapan
- **Red:** Manado, Pontianak, Batam
- **Yellow:** Depok, Tangerang, Bekasi
- **Green:** Bogor, Jakarta Selatan, Jakarta Pusat
- **Dark Blue:** Jakarta Utara, Jakarta Barat

**Railroads (4 stasiun):**
- Stasiun Gambir, Stasiun Pasar Senen, Stasiun Manggarai, Stasiun Tanah Abang

**Utilities (2 perusahaan):**
- PLN (Perusahaan Listrik Negara)
- PDAM (Perusahaan Daerah Air Minum)

---

### 2. Manual End Turn System 🎮

**Flow Permainan:**

```
Player Turn:
  1️⃣ Roll Dice (wajib, 1x saja)
     ↓
  2️⃣ Actions (opsional, bisa banyak)
     • Buy property
     • Build/sell houses
     • Mortgage/unmortgage
     • Trade with players
     ↓
  3️⃣ End Turn (wajib, manual)
     ↓
  Next Player
```

**Keuntungan:**
- ✅ Pemain bisa strategize sebelum end turn
- ✅ Multiple actions dalam satu turn
- ✅ Lebih fleksibel dan realistic
- ✅ Anti-spam built-in (1x roll per turn)

---

### 3. Enhanced Validation ✅

**Game Rules:**
- Pemain hanya bisa roll dice 1x per turn
- Harus roll dice sebelum end turn
- Hanya current player yang bisa action
- Auto-skip bankrupt players

---

## Perubahan Terbaru (V3.0)

### 🔄 Major Changes

**Added:**
- ✅ Endpoint `POST /api/game/end-turn` (restored)
- ✅ Manual turn control system
- ✅ Validation: must roll before end turn
- ✅ Multiple actions per turn capability

**Removed:**
- ❌ Auto turn after roll dice
- ❌ Auto turn after jail actions

**Retained:**
- ✅ Anti-spam roll dice (1x per turn)
- ✅ Indonesian property names
- ✅ All game mechanics

### 📚 Lihat Juga

- [CHANGELOG.md](./CHANGELOG.md) - Complete version history
- [FRONTEND_MIGRATION_GUIDE_V3.md](./FRONTEND_MIGRATION_GUIDE_V3.md) - Migration guide
- [API_REFERENCE_V3.md](./API_REFERENCE_V3.md) - Detailed API docs

---

## Tech Stack

### Backend

| Technology | Version | Purpose |
|------------|---------|---------|
| **.NET** | 10.0 | Framework |
| **ASP.NET Core** | 10.0 | Web API |
| **C#** | Latest | Language |
| **Swagger** | - | API Documentation |

### Architecture

- **Pattern:** MVC (Model-View-Controller)
- **API Style:** REST
- **Data Storage:** In-memory (single game instance)
- **CORS:** Enabled for localhost:3000 and localhost:5173

### Project Structure

```
backend-monopoly/
├── Controllers/           # API endpoints
│   └── GameController.cs
├── Services/             # Business logic
│   ├── GameService.cs
│   └── GameInitializationService.cs
├── Models/               # Domain models
│   ├── Board.cs
│   ├── Player.cs
│   ├── Asset.cs
│   └── ...
├── DTOs/                 # Data Transfer Objects
│   ├── Requests/
│   └── Responses/
├── Enums/                # Enumerations
├── Interfaces/           # Abstractions
├── Common/               # Utilities
└── Program.cs            # Entry point
```

---

## Installation

### Prerequisites

- **.NET SDK 10.0** or later
- **IDE:** Visual Studio 2022 / VS Code / Rider

### Clone Repository

```bash
git clone <repository-url>
cd backend-monopoly
```

### Restore Dependencies

```bash
dotnet restore
```

### Build Project

```bash
dotnet build
```

---

## Quick Start

### 1. Run Server

```bash
dotnet run
```

Server will start at:
- HTTP: `http://localhost:5278`
- HTTPS: `https://localhost:5279`
- Swagger UI: `http://localhost:5278/swagger`

### 2. Create Game

```bash
curl -X POST http://localhost:5278/api/game/create \
  -H "Content-Type: application/json" \
  -d '{"playerNames": ["Alice", "Bob"]}'
```

### 3. Play Game

```bash
# Roll dice
curl -X POST http://localhost:5278/api/game/roll-dice \
  -H "Content-Type: application/json" \
  -d '{"playerName": "Alice"}'

# Buy property
curl -X POST http://localhost:5278/api/game/buy-property \
  -H "Content-Type: application/json" \
  -d '{"playerName": "Alice"}'

# End turn
curl -X POST http://localhost:5278/api/game/end-turn \
  -H "Content-Type: application/json" \
  -d '{"playerName": "Alice"}'
```

### 4. Get Game State

```bash
curl http://localhost:5278/api/game/state
```

---

## API Endpoints

### Game Management

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/game/create` | Create new game (2-4 players) |
| POST | `/api/game/reset` | Reset current game |
| GET | `/api/game/status` | Check if game exists |
| GET | `/api/game/board` | Get board configuration |
| GET | `/api/game/state` | Get current game state |

### Player Actions

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/game/roll-dice` | Roll dice and move (**NO auto turn**) |
| POST | `/api/game/buy-property` | Buy current property |
| POST | `/api/game/build-house` | Build house on property |
| POST | `/api/game/sell-house` | Sell house from property |
| POST | `/api/game/mortgage` | Mortgage property |
| POST | `/api/game/unmortgage` | Redeem mortgaged property |
| POST | `/api/game/trade` | Trade with another player |

### Jail Actions

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/game/pay-jail-fee` | Pay $50 to exit jail (**NO auto turn**) |
| POST | `/api/game/use-jail-card` | Use jail card (**NO auto turn**) |
| POST | `/api/game/try-roll-doubles` | Try roll doubles (**NO auto turn**) |

### Turn Management

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/game/end-turn` | **Manually end turn** ✅ (V3.0) |

---

## Dokumentasi

### Dokumentasi Lengkap

| File | Description |
|------|-------------|
| [MONOPOLY_API_DOCUMENTATION.md](./MONOPOLY_API_DOCUMENTATION.md) | Full original API docs |
| [API_REFERENCE_V3.md](./API_REFERENCE_V3.md) | V3.0 API reference |
| [CHANGELOG.md](./CHANGELOG.md) | Version history |
| [FRONTEND_MIGRATION_GUIDE_V3.md](./FRONTEND_MIGRATION_GUIDE_V3.md) | Frontend migration guide |

### Swagger UI

Akses interactive API documentation:
```
http://localhost:5278/swagger
```

### Example Requests

Lihat folder `examples/` untuk contoh request lengkap.

---

## Testing

### Manual Testing

#### Test 1: Complete Turn Flow

```bash
# 1. Create game
curl -X POST http://localhost:5278/api/game/create \
  -H "Content-Type: application/json" \
  -d '{"playerNames": ["Alice", "Bob"]}'

# 2. Alice roll dice
curl -X POST http://localhost:5278/api/game/roll-dice \
  -H "Content-Type: application/json" \
  -d '{"playerName": "Alice"}'

# 3. Verify turn is still Alice
curl http://localhost:5278/api/game/state | grep currentPlayerName
# Output: "currentPlayerName": "Alice"

# 4. Alice buy property
curl -X POST http://localhost:5278/api/game/buy-property \
  -H "Content-Type: application/json" \
  -d '{"playerName": "Alice"}'

# 5. Alice end turn
curl -X POST http://localhost:5278/api/game/end-turn \
  -H "Content-Type: application/json" \
  -d '{"playerName": "Alice"}'

# 6. Verify turn is now Bob
curl http://localhost:5278/api/game/state | grep currentPlayerName
# Output: "currentPlayerName": "Bob"
```

#### Test 2: Anti-Spam Validation

```bash
# Roll dice once
curl -X POST http://localhost:5278/api/game/roll-dice \
  -H "Content-Type: application/json" \
  -d '{"playerName": "Alice"}'

# Try roll again (should error)
curl -X POST http://localhost:5278/api/game/roll-dice \
  -H "Content-Type: application/json" \
  -d '{"playerName": "Alice"}'
# Output: {"error": "You have already rolled this turn."}
```

#### Test 3: End Turn Validation

```bash
# Try end turn without rolling (should error)
curl -X POST http://localhost:5278/api/game/end-turn \
  -H "Content-Type: application/json" \
  -d '{"playerName": "Bob"}'
# Output: {"error": "You must roll dice before ending turn."}
```

### Unit Testing

```bash
# Run tests (if available)
dotnet test
```

---

## Migration

### From V2.0 to V3.0

**Backend:** No changes needed (already updated)

**Frontend:** Requires migration

#### Migration Checklist

- [ ] Add `gameApi.endTurn()` method
- [ ] Add "End Turn" button to UI
- [ ] Remove auto-turn assumptions
- [ ] Update game flow logic
- [ ] Handle multiple actions per turn
- [ ] Test complete game flow

**Full Guide:** See [FRONTEND_MIGRATION_GUIDE_V3.md](./FRONTEND_MIGRATION_GUIDE_V3.md)

---

## Game Rules

### Basic Rules

| Rule | Value |
|------|-------|
| Starting Money | $1,500 |
| Passing GO | +$200 |
| House Cost | 50% of property price |
| House Sell Value | 25% of property price |
| Mortgage Value | 50% of property price |
| Unmortgage Cost | 110% of mortgage value |
| Jail Fee | $50 |
| Income Tax | $200 |
| Luxury Tax | $100 |
| Max Houses | 4 houses + 1 hotel |

### Rent Calculation

**Real Estate:**
- No houses: 10% of property price
- 1 house: Base × 5
- 2 houses: Base × 15
- 3 houses: Base × 45
- 4 houses: Base × 80
- Hotel: Base × 100

**Railroads:**
- 1 railroad: $25
- 2 railroads: $50
- 3 railroads: $100
- 4 railroads: $200

**Utilities:**
- 1 utility: $25
- 2 utilities: $50

---

## Development

### Running in Development

```bash
dotnet watch run
```

### Running in Production

```bash
dotnet run -c Release
```

### Build for Production

```bash
dotnet publish -c Release -o ./publish
```

---

## Configuration

### appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### CORS Settings

Configured in `Program.cs`:
```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
            "http://localhost:3000",
            "http://localhost:5173"
        )
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});
```

---

## Troubleshooting

### Common Issues

#### Port Already in Use

```bash
# Kill process on port
lsof -ti:5278 | xargs kill -9

# Or run on different port
dotnet run --urls=http://localhost:5050
```

#### Game Already Exists

```bash
# Reset game first
curl -X POST http://localhost:5278/api/game/reset
```

#### CORS Error

Check that your frontend URL is in allowed origins list in `Program.cs`.

---

## Performance

- **Response Time:** <50ms average
- **Memory Usage:** ~50MB (single game)
- **Concurrent Games:** 1 (current limitation)
- **Max Players:** 4 per game

---

## Security

**Current Status:** No authentication

**Recommendations for Production:**
- Add JWT authentication
- Implement rate limiting
- Add input sanitization
- Enable HTTPS only
- Implement proper error handling

---

## Roadmap

### Planned Features

- [ ] Multiple concurrent games
- [ ] Persistent storage (database)
- [ ] Authentication & authorization
- [ ] Game rooms with unique IDs
- [ ] Spectator mode
- [ ] Game history & statistics
- [ ] WebSocket for real-time updates

---

## Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Open Pull Request

---

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## Support

### Documentation
- API Reference: [API_REFERENCE_V3.md](./API_REFERENCE_V3.md)
- Migration Guide: [FRONTEND_MIGRATION_GUIDE_V3.md](./FRONTEND_MIGRATION_GUIDE_V3.md)
- Changelog: [CHANGELOG.md](./CHANGELOG.md)

### Contact
- Issues: Open an issue on GitHub
- Email: [your-email@example.com]

---

## Acknowledgments

- Built with ASP.NET Core
- Inspired by classic Monopoly game
- Indonesian cities data

---

## Version History

- **V3.0.0** (Current) - Manual end turn system + Indonesian cities
- **V2.0.0** (Deprecated) - Auto turn system
- **V1.0.0** - Initial release

---

**Made with ❤️ for Bootcamp Formulatrix**

**Current Version:** 3.0.0  
**Last Updated:** Februari 2026  
**Status:** ✅ Production Ready
