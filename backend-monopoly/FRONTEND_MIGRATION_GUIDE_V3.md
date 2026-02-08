# 🔄 Migration Guide V3 - Manual End Turn (Rollback)

Panduan migrasi untuk mengembalikan fitur **manual end turn** di game Monopoly.

## 📅 Changelog

**Tanggal Update:** Februari 2026

**Versi:** 3.0 (Rollback dari V2.0)

---

## ⚠️ Breaking Changes (Rollback dari Auto Turn)

### 1. ✅ Endpoint `/api/game/end-turn` KEMBALI AKTIF

**Perubahan:** Game **TIDAK** lagi otomatis ganti giliran. Pemain harus **manual end turn**.

**Migration:**

```diff
// ❌ V2.0 (Auto Turn - Tidak berlaku lagi)
await gameApi.rollDice(currentPlayer);
// Turn otomatis berganti

// ✅ V3.0 (Manual Turn - Behavior saat ini)
await gameApi.rollDice(currentPlayer);
// Turn MASIH pemain yang sama

// Pemain bisa buy property, build house, dll
await gameApi.buyProperty(currentPlayer);
await gameApi.buildHouse({ playerName, propertyName });

// Lalu manual end turn
await gameApi.endTurn(currentPlayer);
// Sekarang baru turn berganti
```

**Impact:**
- ✅ **Tambah kembali** `gameApi.endTurn()` di frontend
- ✅ **Tambah kembali** button "End Turn" di UI
- ✅ Roll dice **TIDAK** otomatis ganti turn
- ✅ Pemain bisa melakukan banyak aksi setelah roll dice

---

## 🎮 Game Flow Baru (V3.0)

### **Turn Structure:**

```
Player1 Turn:
├─ 1. Roll Dice (WAJIB - 1x saja)
│  ├─ Dice rolled
│  ├─ Player moved
│  ├─ Tile effect processed
│  └─ ✅ MASIH Turn Player1
│
├─ 2. Actions (Opsional - bisa banyak)
│  ├─ Buy property (jika landed di property kosong)
│  ├─ Build house on owned properties
│  ├─ Sell house
│  ├─ Mortgage property
│  ├─ Unmortgage property
│  └─ Trade with other players
│
└─ 3. End Turn (WAJIB - Manual)
   ├─ Call POST /api/game/end-turn
   ├─ Validasi: harus sudah roll dice
   └─ ⚡ Turn berganti ke Player2

Player2 Turn:
(repeat...)
```

---

## ✅ Validasi & Rules

| **Action** | **Restriction** | **Error Message** |
|------------|-----------------|-------------------|
| **Roll Dice** | Hanya 1x per turn | "You have already rolled this turn." |
| **End Turn** | Harus sudah roll dice | "You must roll dice before ending turn." |
| **Buy Property** | Setelah roll dice, masih turn yang sama | ✅ Bisa dilakukan |
| **Build House** | Kapan saja dalam turn | ✅ Bisa dilakukan |
| **Jail Actions** | Tidak auto turn | ✅ Harus manual end turn |

---

## 🔄 Comparison: V2.0 vs V3.0

| **Aspect** | **V2.0 (Auto Turn)** | **V3.0 (Manual Turn)** |
|------------|---------------------|------------------------|
| Roll Dice | Auto NextTurn() ❌ | Tetap turn pemain ✅ |
| Jail Actions | Auto NextTurn() ❌ | Tetap turn pemain ✅ |
| Buy Property | Tidak bisa (sudah beda turn) | Bisa ✅ |
| End Turn Endpoint | ❌ Dihapus (404) | ✅ Aktif (200 OK) |
| Anti-Spam Roll | Ada ✅ | Tetap ada ✅ |
| Validasi End Turn | - | Harus sudah roll ✅ |
| Nama Properti | Kota Indonesia ✅ | Tetap Indonesia ✅ |

---

## 📝 Updated API Behavior

### **1. POST /api/game/roll-dice**

**Request:**
```json
{
  "playerName": "Player1"
}
```

**Response (Success):**
```json
{
  "dice1": 4,
  "dice2": 5,
  "total": 9,
  "isDouble": false,
  "newPosition": 9,
  "landedTile": "Makassar"
}
```

**Behavior:**
- ✅ Player roll dice
- ✅ Player moved to new position
- ✅ Tile effect processed
- ✅ Turn **MASIH** Player1 (tidak auto turn)
- ✅ Player bisa melakukan aksi lain

**Anti-Spam:**
```json
// Jika player sudah roll di turn ini:
{
  "error": "You have already rolled this turn."
}
```

---

### **2. POST /api/game/end-turn** ✅ (KEMBALI AKTIF)

**Request:**
```json
{
  "playerName": "Player1"
}
```

**Response (Success):**
```json
{
  "success": true,
  "message": "Turn ended. Now it's Player2's turn."
}
```

**Response (Error - Belum Roll):**
```json
{
  "error": "You must roll dice before ending turn."
}
```

**Response (Error - Bukan Turn Pemain):**
```json
{
  "error": "It's not Player1's turn. Current player is Player2."
}
```

**Behavior:**
- ✅ Validasi: pemain harus sudah roll dice
- ✅ Validasi: harus turn pemain tersebut
- ✅ Panggil `NextTurn()` → ganti ke pemain selanjutnya
- ✅ Reset flag `_hasRolledThisTurn` untuk pemain baru

---

### **3. POST /api/game/buy-property**

**Behavior (V3.0):**
- ✅ Bisa dipanggil setelah roll dice
- ✅ Turn **MASIH** pemain yang sama
- ✅ Pemain bisa aksi lain setelah buy property
- ✅ Harus manual end turn untuk ganti giliran

**Example Flow:**
```typescript
// 1. Roll dice
await gameApi.rollDice("Player1");
// currentPlayerName = "Player1"

// 2. Buy property
await gameApi.buyProperty("Player1");
// currentPlayerName = "Player1" (masih sama)

// 3. Build house
await gameApi.buildHouse({ playerName: "Player1", propertyName: "Medan" });
// currentPlayerName = "Player1" (masih sama)

// 4. Manual end turn
await gameApi.endTurn("Player1");
// currentPlayerName = "Player2" (baru berganti!)
```

---

### **4. Jail Actions**

**Affected Endpoints:**
- `POST /api/game/pay-jail-fee`
- `POST /api/game/use-jail-card`
- `POST /api/game/try-roll-doubles`

**Behavior (V3.0):**
- ✅ Aksi jail **TIDAK** auto turn
- ✅ Pemain masih bisa aksi lain setelah keluar dari jail
- ✅ Harus manual end turn

**Example:**
```typescript
// Player di jail
await gameApi.payJailFee("Player1");
// currentPlayerName = "Player1" (masih sama)

// Player keluar dari jail, bisa aksi lain
await gameApi.buildHouse({ playerName: "Player1", propertyName: "Bandung" });

// Manual end turn
await gameApi.endTurn("Player1");
// currentPlayerName = "Player2"
```

---

## 🎯 Frontend Implementation Guide

### **Updated API Service:**

```typescript
// src/services/api.ts

export const gameApi = {
  // ... (methods lain tetap sama)
  
  rollDice: (playerName: string) =>
    api.post<RollDiceResponse>('/roll-dice', { playerName }),
  
  buyProperty: (playerName: string) =>
    api.post<ActionResultResponse>('/buy-property', { playerName }),
  
  // ✅ TAMBAH KEMBALI
  endTurn: (playerName: string) =>
    api.post<ActionResultResponse>('/end-turn', { playerName }),
  
  // Jail actions (tidak auto turn)
  payJailFee: (playerName: string) =>
    api.post<ActionResultResponse>('/pay-jail-fee', { playerName }),
  
  useJailCard: (playerName: string) =>
    api.post<ActionResultResponse>('/use-jail-card', { playerName }),
  
  tryRollDoubles: (playerName: string) =>
    api.post<RollDiceResponse>('/try-roll-doubles', { playerName }),
};
```

---

### **React Component Example:**

```typescript
// src/components/GameBoard.tsx

const GameBoard = () => {
  const { gameState, refreshGameState } = useGameState();
  const [hasRolled, setHasRolled] = useState(false);

  const handleRollDice = async () => {
    try {
      const result = await gameApi.rollDice(gameState.currentPlayerName);
      
      // Tampilkan hasil roll
      showDiceAnimation(result.dice1, result.dice2);
      
      // Refresh state (turn masih sama)
      await refreshGameState();
      
      // Set flag sudah roll
      setHasRolled(true);
    } catch (error) {
      console.error(error.response?.data?.error);
    }
  };

  const handleBuyProperty = async () => {
    try {
      await gameApi.buyProperty(gameState.currentPlayerName);
      await refreshGameState();
      // Turn masih sama, bisa aksi lain
    } catch (error) {
      console.error(error.response?.data?.error);
    }
  };

  const handleEndTurn = async () => {
    try {
      await gameApi.endTurn(gameState.currentPlayerName);
      await refreshGameState();
      
      // Reset flag untuk turn baru
      setHasRolled(false);
      
      // Turn sudah berganti
    } catch (error) {
      console.error(error.response?.data?.error);
    }
  };

  const isMyTurn = gameState?.currentPlayerName === myPlayerName;
  const canRoll = isMyTurn && !hasRolled;
  const canEndTurn = isMyTurn && hasRolled;

  return (
    <div>
      {/* Roll Dice Button */}
      <button 
        onClick={handleRollDice} 
        disabled={!canRoll}
      >
        Roll Dice
      </button>

      {/* Buy Property Button */}
      <button 
        onClick={handleBuyProperty} 
        disabled={!isMyTurn || !canBuyProperty}
      >
        Buy Property
      </button>

      {/* End Turn Button - KEMBALI AKTIF */}
      <button 
        onClick={handleEndTurn} 
        disabled={!canEndTurn}
      >
        End Turn
      </button>
    </div>
  );
};
```

---

## 🧪 Testing Guidelines

### **Test Scenario 1: Normal Turn Flow**

```typescript
// 1. Player1 roll dice
await gameApi.rollDice("Player1");
const state1 = await gameApi.getState();
expect(state1.currentPlayerName).toBe("Player1"); // Masih Player1

// 2. Player1 buy property
await gameApi.buyProperty("Player1");
const state2 = await gameApi.getState();
expect(state2.currentPlayerName).toBe("Player1"); // Masih Player1

// 3. Player1 end turn
await gameApi.endTurn("Player1");
const state3 = await gameApi.getState();
expect(state3.currentPlayerName).toBe("Player2"); // Baru berganti!
```

---

### **Test Scenario 2: Anti-Spam Roll**

```typescript
// 1. Player1 roll dice
await gameApi.rollDice("Player1");

// 2. Player1 coba roll lagi
try {
  await gameApi.rollDice("Player1"); // Error!
  fail("Should have thrown error");
} catch (error) {
  expect(error.response.data.error).toContain("already rolled");
}
```

---

### **Test Scenario 3: Validasi End Turn (Belum Roll)**

```typescript
// Player1 coba end turn tanpa roll dice
try {
  await gameApi.endTurn("Player1"); // Error!
  fail("Should have thrown error");
} catch (error) {
  expect(error.response.data.error).toContain("must roll dice");
}

// Setelah roll, baru bisa end turn
await gameApi.rollDice("Player1");
await gameApi.endTurn("Player1"); // Success!
```

---

### **Test Scenario 4: Jail Actions Tidak Auto Turn**

```typescript
// Player di jail, bayar denda
await gameApi.payJailFee("Player1");

// Turn masih Player1
const state = await gameApi.getState();
expect(state.currentPlayerName).toBe("Player1");

// Player bisa aksi lain
await gameApi.buildHouse({ playerName: "Player1", propertyName: "Medan" });

// Manual end turn
await gameApi.endTurn("Player1");

// Baru berganti
const newState = await gameApi.getState();
expect(newState.currentPlayerName).toBe("Player2");
```

---

## ✅ Migration Checklist

### **Code Changes:**

- [x] **Tambah kembali** `gameApi.endTurn()` method
- [x] **Tambah kembali** button "End Turn" di UI
- [x] **Hapus** asumsi auto turn setelah roll dice
- [x] **Update** flow: Roll → Actions → Manual End Turn
- [x] **Tambah** state tracking `hasRolled` di frontend
- [x] **Enable** button end turn hanya jika sudah roll
- [x] **Disable** button roll jika sudah roll di turn ini

### **UI/UX Changes:**

- [x] Tampilkan button "End Turn" di action panel
- [x] Disable button end turn jika belum roll dice
- [x] Show indicator "You must roll dice first" jika belum roll
- [x] Show indicator "You can perform other actions" setelah roll
- [x] Enable semua action buttons setelah roll (buy, build, trade, dll)

### **Testing:**

- [x] Test roll dice tidak auto turn
- [x] Test anti-spam roll tetap aktif
- [x] Test buy property setelah roll dice
- [x] Test end turn setelah roll dice
- [x] Test error end turn jika belum roll
- [x] Test jail actions tidak auto turn
- [x] Full game flow dari awal sampai akhir

---

## 🎨 Updated TypeScript Types

```typescript
// availableActions sekarang include 'end-turn'
type AvailableAction =
  | 'roll-dice'
  | 'buy-property'
  | 'build-house'
  | 'sell-house'
  | 'mortgage-property'
  | 'unmortgage-property'
  | 'trade'
  | 'pay-jail-fee'
  | 'use-jail-card'
  | 'try-roll-doubles'
  | 'end-turn'; // ✅ KEMBALI AKTIF
```

---

## 🏙️ Nama Properti (Tidak Berubah)

Nama properti **tetap menggunakan kota Indonesia:**

- Medan, Palembang
- Semarang, Surabaya, Makassar
- Bandung, Yogyakarta, Solo
- Denpasar, Malang, Balikpapan
- Manado, Pontianak, Batam
- Depok, Tangerang, Bekasi
- Bogor, Jakarta Selatan, Jakarta Pusat
- Jakarta Utara, Jakarta Barat
- Stasiun Gambir, Stasiun Pasar Senen, Stasiun Manggarai, Stasiun Tanah Abang
- PLN, PDAM

---

## 🚨 Common Pitfalls

### **1. Lupa End Turn**

```typescript
// ❌ SALAH - Lupa end turn
await gameApi.rollDice(currentPlayer);
await gameApi.buyProperty(currentPlayer);
// Game stuck! Turn tidak berganti

// ✅ BENAR
await gameApi.rollDice(currentPlayer);
await gameApi.buyProperty(currentPlayer);
await gameApi.endTurn(currentPlayer); // Wajib!
```

### **2. Coba End Turn Sebelum Roll**

```typescript
// ❌ SALAH
await gameApi.endTurn(currentPlayer); // Error: must roll dice first

// ✅ BENAR
await gameApi.rollDice(currentPlayer);
await gameApi.endTurn(currentPlayer);
```

### **3. Asumsi Auto Turn Setelah Buy Property**

```typescript
// ❌ SALAH - Asumsi auto turn
await gameApi.buyProperty(currentPlayer);
// Langsung enable button untuk pemain lain
// SALAH! Turn masih pemain yang sama

// ✅ BENAR
await gameApi.buyProperty(currentPlayer);
await refreshGameState();
// Cek state, turn masih sama
// Pemain masih bisa aksi lain
await gameApi.buildHouse({ ... });
// Lalu manual end turn
await gameApi.endTurn(currentPlayer);
```

---

## 🆘 Support

Jika ada pertanyaan atau masalah saat migrasi:

1. Cek dokumentasi API lengkap: `MONOPOLY_API_DOCUMENTATION.md`
2. Test endpoint dengan Swagger UI: `http://localhost:5278/swagger`
3. Test dengan cURL (lihat contoh di dokumentasi utama)

---

## 📊 Summary

**V3.0 Rollback Changes:**

✅ **Dikembalikan:**
- Endpoint `POST /api/game/end-turn`
- Manual end turn flow
- Buy property setelah roll dice

✅ **Dipertahankan:**
- Anti-spam roll dice (1x per turn)
- Nama properti kota Indonesia
- Validasi turn pemain
- Dictionary `_hasRolledThisTurn`

❌ **Dihapus:**
- Auto turn setelah roll dice
- Auto turn setelah jail actions

---

**Last Updated:** Februari 2026  
**Version:** 3.0 (Manual End Turn)  
**Status:** ✅ Active & Tested
