# 🎲 Monopoly Frontend - React TypeScript

Frontend UI sederhana untuk game Monopoly yang berkomunikasi dengan backend ASP.NET Core API.

## 🚀 Quick Start

### Prerequisites
- Node.js v18+ atau v20+
- Backend Monopoly API running di `http://localhost:5278`

### Installation

```bash
# Install dependencies
npm install

# Start development server
npm run dev
```

Frontend akan berjalan di **http://localhost:5173**

### Build for Production

```bash
npm run build
npm run preview
```

---

## 📁 Project Structure

```
frontend-monopoly/
├── src/
│   ├── components/
│   │   ├── Board/              # Komponen papan game
│   │   │   ├── Board.tsx       # 11x11 CSS Grid board
│   │   │   ├── Tile.tsx        # Tile individual (40 tiles)
│   │   │   └── PlayerToken.tsx # Token pemain
│   │   ├── Player/             # Info pemain
│   │   │   ├── PlayerCard.tsx  # Card info pemain
│   │   │   └── PropertyList.tsx # List properti owned
│   │   ├── Actions/            # Panel aksi game
│   │   │   ├── ActionPanel.tsx # Tombol aksi dinamis
│   │   │   ├── DiceDisplay.tsx # Tampilan hasil dadu
│   │   │   ├── JailOptions.tsx # Aksi jail
│   │   │   ├── PropertyActions.tsx # Build/Sell house
│   │   │   ├── MortgagePanel.tsx # Mortgage/Unmortgage
│   │   │   └── TradeModal.tsx # Trading interface
│   │   └── Game/               # Game flow
│   │       ├── GameSetup.tsx   # Setup awal game
│   │       ├── GameOver.tsx    # Winner screen
│   │       └── TurnIndicator.tsx # Turn display
│   ├── hooks/
│   │   └── useGameState.ts     # Custom hook untuk state management
│   ├── services/
│   │   └── api.ts              # Axios API client
│   ├── types/
│   │   └── index.ts            # TypeScript interfaces
│   ├── utils/
│   │   ├── boardLayout.ts      # Grid position mapping
│   │   └── propertyColors.ts   # Color mapping
│   ├── App.tsx                 # Main component
│   ├── main.tsx                # Entry point
│   └── index.css               # Tailwind CSS
└── package.json
```

---

## 🎮 Fitur yang Diimplementasikan

### ✅ Core Gameplay
- [x] **Create Game** - 2-4 players
- [x] **Game Board** - 40 tiles dengan CSS Grid layout
- [x] **Player Tokens** - Colored circles with initials
- [x] **Roll Dice** - Dengan display hasil
- [x] **Buy Property** - Beli properti yang belum ada owner
- [x] **Pay Rent** - Otomatis dari backend
- [x] **End Turn** - Pindah ke player berikutnya
- [x] **Game Over** - Winner screen dengan final standings

### ✅ Jail System
- [x] **Go to Jail** - Tile "Go To Jail" mengirim ke penjara
- [x] **Pay Jail Fee** - Bayar $50 untuk keluar
- [x] **Use Jail Card** - Pakai "Get Out of Jail Free" card
- [x] **Try Roll Doubles** - Coba lempar dadu kembar

### ✅ Property Management
- [x] **Build House** - Bangun rumah di properti (1-4 houses + hotel)
- [x] **Sell House** - Jual rumah
- [x] **Mortgage Property** - Gadaikan properti untuk uang
- [x] **Unmortgage Property** - Tebus properti yang digadaikan

### ✅ Advanced Features
- [x] **Trade** - Trading properties & money antar player
- [x] **Property List** - Expandable list per player
- [x] **Real-time State** - Auto-refresh setelah action
- [x] **Toast Notifications** - Feedback untuk semua aksi
- [x] **Responsive Design** - Desktop & mobile friendly

---

## 🎯 Cara Bermain

### 1. Setup Game
1. Pastikan backend running di `http://localhost:5278`
2. Buka http://localhost:5173
3. Pilih jumlah player (2-4)
4. Isi nama masing-masing player
5. Klik "Start Game"

### 2. Gameplay Loop
**Setiap Turn:**
1. Klik "🎲 Roll Dice"
2. Player token bergerak otomatis
3. Aksi muncul berdasarkan tile:
   - **Unowned Property** → "💰 Buy Property"
   - **Owned Property** → Bayar rent otomatis
   - **Jail** → Pilih aksi jail
   - **Chance/Community Chest** → Ambil kartu otomatis
4. (Optional) Build house, mortgage, trade
5. Klik "✅ End Turn"

### 3. Win Condition
Game berakhir ketika hanya 1 player yang tidak bankrupt.

---

## 🛠️ Development

### Available Scripts

```bash
# Development server
npm run dev

# Build for production
npm run build

# Preview production build
npm run preview
```

### Connecting to Different Backend

Edit `src/services/api.ts`:

```typescript
const BASE_URL = 'http://your-backend-url:port/api/game';
```

---

## 🐛 Known Issues

1. **CORS**: Pastikan backend sudah enable CORS untuk `localhost:5173`
2. **Backend URL**: Sesuaikan base URL jika backend berjalan di port lain

---

**Happy Playing! 🎲🏠💰**

