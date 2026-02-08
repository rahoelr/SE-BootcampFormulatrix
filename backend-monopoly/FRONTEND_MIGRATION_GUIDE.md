# 🔄 Migration Guide - Auto Turn Update

Panduan migrasi untuk frontend developer terkait perubahan major pada game logic.

## 📅 Changelog

**Tanggal Update:** Februari 2026

**Versi:** 2.0

---

## ⚠️ Breaking Changes

### 1. ❌ Endpoint `/api/game/end-turn` DIHAPUS

**Alasan:** Game sekarang otomatis ganti giliran setelah pemain selesai roll dice atau aksi jail.

**Migration:**

```diff
// ❌ SEBELUM (Tidak berfungsi lagi)
await gameApi.endTurn(currentPlayer);

// ✅ SESUDAH (Tidak perlu manual end turn)
// Setelah roll dice, turn otomatis berganti
await gameApi.rollDice(currentPlayer);
// Turn sudah berganti, cek state untuk pemain selanjutnya
```

**Impact:**
- Hapus semua pemanggilan `gameApi.endTurn()` dari kode frontend
- Hapus button "End Turn" dari UI
- State `currentPlayerName` akan otomatis berubah setelah roll dice

---

### 2. 🔄 Auto Turn Flow

#### **a. Roll Dice → Auto Turn**

**Behavior Baru:**
- Setelah roll dice, giliran **OTOMATIS** berpindah ke pemain berikutnya
- Pemain tidak bisa roll dice berkali-kali dalam 1 turn
- Jika coba roll lagi, akan error: `"You have already rolled this turn."`

**Code Example:**

```typescript
// Sebelum roll dice
console.log(gameState.currentPlayerName); // "Player1"

await gameApi.rollDice("Player1");

// Fetch state lagi
const newState = await gameApi.getState();
console.log(newState.currentPlayerName); // "Player2" (otomatis berganti!)
```

**UI Pattern:**

```typescript
const handleRollDice = async () => {
  try {
    const rollResult = await gameApi.rollDice(gameState.currentPlayerName);
    
    // Tampilkan animasi dadu
    showDiceAnimation(rollResult.dice1, rollResult.dice2);
    
    // Refresh state (turn sudah berganti)
    await refreshGameState();
    
    // UI akan otomatis update ke pemain selanjutnya
  } catch (error) {
    console.error(error.response?.data?.error);
  }
};
```

---

#### **b. Jail Actions → Auto Turn**

**Aksi yang otomatis ganti turn:**
- `POST /api/game/pay-jail-fee`
- `POST /api/game/use-jail-card`
- `POST /api/game/try-roll-doubles`

**Code Example:**

```typescript
// Bayar denda jail
await gameApi.payJailFee(currentPlayer);
// Turn sudah berganti otomatis

// Pakai kartu
await gameApi.useJailCard(currentPlayer);
// Turn sudah berganti otomatis

// Coba roll doubles
await gameApi.tryRollDoubles(currentPlayer);
// Turn sudah berganti otomatis (berhasil atau gagal)
```

---

#### **c. Buy Property → TIDAK Auto Turn**

**PENTING:** Buy property **TIDAK** otomatis ganti turn!

**Alasan:** Pemain mungkin ingin:
- Build house setelah buy property
- Mortgage properti lain untuk dapat uang
- Trade dengan pemain lain

**Code Example:**

```typescript
// Buy property
await gameApi.buyProperty(currentPlayer);
// Turn MASIH pemain yang sama

// Masih bisa lakukan aksi lain
await gameApi.buildHouse({ 
  playerName: currentPlayer, 
  propertyName: "Medan" 
});
// Masih turn yang sama

// Atau aksi lainnya...
```

**UI Handling:**

```typescript
// Setelah buy property, pemain masih punya kontrol
const handleBuyProperty = async () => {
  await gameApi.buyProperty(currentPlayer);
  await refreshGameState();
  
  // Turn masih sama, tampilkan opsi lain
  // - Build house?
  // - Trade?
  // - Dll
};
```

---

### 3. 🏙️ Nama Properti → Kota Indonesia

**Semua properti sekarang menggunakan nama kota Indonesia.**

#### **Mapping Lengkap:**

| **Group** | **Nama Lama** | **Nama Baru** | **Harga** |
|-----------|---------------|---------------|-----------|
| **Brown** | Mediterranean Avenue | Medan | $60 |
|  | Baltic Avenue | Palembang | $60 |
| **Light Blue** | Oriental Avenue | Semarang | $100 |
|  | Vermont Avenue | Surabaya | $100 |
|  | Connecticut Avenue | Makassar | $120 |
| **Pink** | St. Charles Place | Bandung | $140 |
|  | States Avenue | Yogyakarta | $140 |
|  | Virginia Avenue | Solo | $160 |
| **Orange** | St. James Place | Denpasar | $180 |
|  | Tennessee Avenue | Malang | $180 |
|  | New York Avenue | Balikpapan | $200 |
| **Red** | Kentucky Avenue | Manado | $220 |
|  | Indiana Avenue | Pontianak | $220 |
|  | Illinois Avenue | Batam | $240 |
| **Yellow** | Atlantic Avenue | Depok | $260 |
|  | Ventnor Avenue | Tangerang | $260 |
|  | Marvin Gardens | Bekasi | $280 |
| **Green** | Pacific Avenue | Bogor | $300 |
|  | North Carolina Avenue | Jakarta Selatan | $300 |
|  | Pennsylvania Avenue | Jakarta Pusat | $320 |
| **Dark Blue** | Park Place | Jakarta Utara | $350 |
|  | Boardwalk | Jakarta Barat | $400 |
| **Railroad** | Reading Railroad | Stasiun Gambir | $200 |
|  | Pennsylvania Railroad | Stasiun Pasar Senen | $200 |
|  | B&O Railroad | Stasiun Manggarai | $200 |
|  | Short Line Railroad | Stasiun Tanah Abang | $200 |
| **Utility** | Electric Company | PLN | $150 |
|  | Water Works | PDAM | $150 |

**Migration:**

```typescript
// ❌ SEBELUM
const property = "Mediterranean Avenue";

// ✅ SESUDAH
const property = "Medan";
```

**UI Update:**
- Update hardcoded property names di frontend
- Update color mapping jika ada
- Asset images perlu diganti (jika ada gambar properti)

---

## ✅ Updated API Flow

### **Flow Normal Turn (Tanpa Jail):**

```
1. Frontend: GET /api/game/state
   Response: currentPlayerName = "Player1"

2. Player1 klik "Roll Dice"

3. Frontend: POST /api/game/roll-dice (playerName: "Player1")
   Response: Dice values + new position
   
   ⚡ Backend otomatis panggil NextTurn()

4. Frontend: GET /api/game/state
   Response: currentPlayerName = "Player2" (sudah berganti!)

5. Player2 sekarang yang bermain
```

### **Flow dengan Buy Property:**

```
1. Frontend: POST /api/game/roll-dice (playerName: "Player1")
   Response: Landed on "Medan"
   
   ⚡ Backend otomatis NextTurn() → Player2 turn

2. Frontend: GET /api/game/state
   Response: currentPlayerName = "Player2"

3. Player2 roll dice, mendarat di "Surabaya" (property kosong)
   Frontend: POST /api/game/roll-dice (playerName: "Player2")
   
   ⚡ Backend otomatis NextTurn() → Player3 turn... WAIT!
   
   Tapi Player2 ingin beli property!

❌ SALAH FLOW! Player2 sudah kehilangan turn!

✅ BENAR FLOW:

1. Player2 mendarat di tile setelah roll
2. Sebelum turn berganti, Player2 punya kesempatan:
   - Lihat tile yang didarat
   - Jika property kosong, bisa buy (TIDAK auto turn)
   - Jika tidak beli atau aksi lain selesai, MANUAL pass turn

⚠️ CATATAN PENTING:
   Buy property TIDAK auto turn karena pemain masih bisa:
   - Build house
   - Mortgage properti lain
   - Trade
   
   Pemain harus SELESAI semua aksinya baru turn berganti.
```

**WAIT! Ada masalah di flow ini!**

Berdasarkan requirement user:
- Roll dice → auto turn ✅
- Jail actions → auto turn ✅
- Buy property → TIDAK auto turn ✅

**Tapi ada conflict:**
Jika roll dice auto turn, kapan pemain bisa buy property?

**Solusi yang benar:**
Roll dice TIDAK langsung auto turn. Auto turn terjadi setelah pemain selesai dengan semua aksi di tile tersebut.

**ATAU requirement user memang:**
- Roll dice langsung auto turn (pemain harus cepat buy property sebelum turn berganti?)

Mari saya perbaiki dokumentasi ini sesuai implementasi yang benar.

---

## 🔧 Updated API Flow (REVISED)

Berdasarkan implementasi kode:

### **Actual Implementation:**

1. **Roll Dice** → Langsung **NextTurn()** dipanggil
2. **Jail Actions** → Langsung **NextTurn()** dipanggil  
3. **Buy Property** → **TIDAK** auto turn

**Konsekuensi:**
- Setelah roll dice, giliran LANGSUNG berganti ke pemain berikutnya
- Pemain TIDAK bisa buy property setelah roll dice karena sudah bukan turn-nya

**Ini berarti ada bug/design flaw!**

**Kemungkinan Fix:**
1. Pindahkan `NextTurn()` dari `ExecuteRollDice()` 
2. Biarkan pemain aksi dulu (buy property, build house, dll)
3. Panggil auto turn setelah pemain "selesai" dengan turn-nya

**ATAU** jika memang intended behavior:
- Buy property harus dipanggil SEBELUM roll dice
- Setelah roll dice, turn langsung berganti (pemain tidak bisa berbuat apa-apa)

Mari saya konfirmasi dengan user terlebih dahulu tentang flow yang diinginkan.

---

## 💡 Rekomendasi untuk Frontend

Karena ada ambiguitas di flow, saya rekomendasikan 2 opsi:

### **Opsi A: Roll Dice Tidak Auto Turn (Lebih Masuk Akal)**

```typescript
// Roll dice TIDAK langsung ganti turn
await gameApi.rollDice(currentPlayer);

// Pemain bisa buy property jika mendarat di property kosong
if (canBuyProperty) {
  await gameApi.buyProperty(currentPlayer);
}

// Setelah selesai semua aksi, manual end turn
// Tapi endpoint end-turn sudah dihapus!
// Jadi perlu ada cara lain untuk signal "selesai turn"
```

### **Opsi B: Roll Dice Auto Turn (Sesuai Kode Saat Ini)**

```typescript
// Roll dice langsung ganti turn
await gameApi.rollDice(currentPlayer);

// Turn sudah berganti, pemain tidak bisa berbuat apa-apa lagi
// Buy property TIDAK bisa dilakukan setelah roll dice

// Aksi property management dilakukan SEBELUM roll:
await gameApi.buildHouse({ playerName, propertyName });
await gameApi.mortgage({ playerName, propertyName });

// Setelah selesai manage property, baru roll dice
await gameApi.rollDice(playerName); // Langsung ganti turn
```

**Berdasarkan implementasi kode saat ini, Opsi B yang aktif.**

---

## 🎮 Gameplay Flow (Based on Current Implementation)

### **Turn Structure:**

```
Player1 Turn:
├─ 1. Manage Properties (opsional)
│  ├─ Build house on existing properties
│  ├─ Sell house
│  ├─ Mortgage property
│  └─ Trade with other players
│
├─ 2. Roll Dice (WAJIB)
│  ├─ Dice rolled
│  ├─ Player moved
│  ├─ Tile effect processed
│  └─ ⚡ AUTO NextTurn() ⚡
│
└─ 3. Turn Ended → Player2 Turn

Player2 Turn:
(repeat...)
```

**Key Points:**
- Semua property management dilakukan SEBELUM roll dice
- Setelah roll dice, turn LANGSUNG berganti
- Player TIDAK bisa buy property setelah mendarat di tile
- Buy property endpoint tetap ada tapi tidak berguna dalam flow normal

**Catatan:** Ini mungkin bukan intended behavior. Perlu konfirmasi dengan user.

---

## 🚨 Common Pitfalls

### **1. Lupa Refresh State Setelah Roll Dice**

```typescript
// ❌ SALAH - State tidak update
await gameApi.rollDice(currentPlayer);
// Masih pakai state lama (currentPlayer tidak berubah)

// ✅ BENAR
await gameApi.rollDice(currentPlayer);
await refreshGameState(); // Update state!
```

### **2. Coba Buy Property Setelah Roll Dice**

```typescript
// ❌ SALAH - Sudah bukan turn pemain
await gameApi.rollDice("Player1"); // Turn berganti ke Player2
await gameApi.buyProperty("Player1"); // ERROR! Bukan turn Player1

// ✅ BENAR - Buy property sebelum roll? (tapi tidak masuk akal)
// Atau tunggu konfirmasi flow yang benar dari backend team
```

### **3. Hardcoded Property Names**

```typescript
// ❌ SALAH - Nama lama
if (tileName === 'Mediterranean Avenue') { ... }

// ✅ BENAR - Nama baru
if (tileName === 'Medan') { ... }
```

---

## 🔧 Updated TypeScript Types

### **Removed:**

```typescript
// ❌ DIHAPUS dari AvailableAction
type AvailableAction = 'end-turn'; // Tidak ada lagi
```

### **Updated AvailableAction Type:**

```typescript
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
  | 'try-roll-doubles';
  // 'end-turn' DIHAPUS
```

### **Property Names Type:**

```typescript
type PropertyName =
  | 'Medan' | 'Palembang'
  | 'Semarang' | 'Surabaya' | 'Makassar'
  | 'Bandung' | 'Yogyakarta' | 'Solo'
  | 'Denpasar' | 'Malang' | 'Balikpapan'
  | 'Manado' | 'Pontianak' | 'Batam'
  | 'Depok' | 'Tangerang' | 'Bekasi'
  | 'Bogor' | 'Jakarta Selatan' | 'Jakarta Pusat'
  | 'Jakarta Utara' | 'Jakarta Barat'
  | 'Stasiun Gambir' | 'Stasiun Pasar Senen' 
  | 'Stasiun Manggarai' | 'Stasiun Tanah Abang'
  | 'PLN' | 'PDAM';
```

---

## 🎨 Updated Property Color Groups

```typescript
const propertyColors: Record<string, string[]> = {
  brown: ['Medan', 'Palembang'],
  lightBlue: ['Semarang', 'Surabaya', 'Makassar'],
  pink: ['Bandung', 'Yogyakarta', 'Solo'],
  orange: ['Denpasar', 'Malang', 'Balikpapan'],
  red: ['Manado', 'Pontianak', 'Batam'],
  yellow: ['Depok', 'Tangerang', 'Bekasi'],
  green: ['Bogor', 'Jakarta Selatan', 'Jakarta Pusat'],
  darkBlue: ['Jakarta Utara', 'Jakarta Barat'],
};
```

---

## 📝 Updated API Service

```typescript
// src/services/api.ts

export const gameApi = {
  // ... (methods lain tetap sama)
  
  // ❌ HAPUS METHOD INI
  // endTurn: (playerName: string) => 
  //   api.post<ActionResultResponse>('/end-turn', { playerName }),
  
  // ✅ Method lain tidak berubah
  rollDice: (playerName: string) =>
    api.post<RollDiceResponse>('/roll-dice', { playerName }),
  
  buyProperty: (playerName: string) =>
    api.post<ActionResultResponse>('/buy-property', { playerName }),
  
  // Jail actions (sekarang auto turn)
  payJailFee: (playerName: string) =>
    api.post<ActionResultResponse>('/pay-jail-fee', { playerName }),
  
  useJailCard: (playerName: string) =>
    api.post<ActionResultResponse>('/use-jail-card', { playerName }),
  
  tryRollDoubles: (playerName: string) =>
    api.post<RollDiceResponse>('/try-roll-doubles', { playerName }),
};
```

---

## 🧪 Testing Guidelines

### **Test Auto Turn:**

```typescript
// Test: Roll dice auto ganti turn
const beforeState = await gameApi.getState();
expect(beforeState.currentPlayerName).toBe('Player1');

await gameApi.rollDice('Player1');

const afterState = await gameApi.getState();
expect(afterState.currentPlayerName).toBe('Player2'); // Auto ganti!
```

### **Test Anti-Spam Roll:**

```typescript
// Test: Tidak bisa roll 2x
await gameApi.rollDice('Player1');

try {
  await gameApi.rollDice('Player1'); // Error!
  fail('Should have thrown error');
} catch (error) {
  expect(error.response.data.error).toContain('already rolled');
}
```

### **Test Property Names:**

```typescript
// Test: Nama kota Indonesia
const board = await gameApi.getBoard();
const property1 = board.tiles[1];
expect(property1.name).toBe('Medan'); // Bukan 'Mediterranean Avenue'
```

---

## ✅ Checklist Migration

### **Code Changes:**

- [ ] Hapus semua `gameApi.endTurn()` calls
- [ ] Hapus button "End Turn" dari UI
- [ ] Update property names: Amerika → Indonesia
- [ ] Update property color mapping
- [ ] Update TypeScript types (`AvailableAction`, `PropertyName`)
- [ ] Tambah auto refresh state setelah roll dice
- [ ] Tambah auto refresh state setelah jail actions
- [ ] Update test cases

### **UI/UX Changes:**

- [ ] Hapus "End Turn" button
- [ ] Update tile names di board
- [ ] Update property card names
- [ ] Update asset images (jika ada)
- [ ] Tampilkan indikator "Turn Ended" setelah roll dice
- [ ] Tampilkan indikator "Next Player" setelah turn berganti

### **Testing:**

- [ ] Test auto turn setelah roll dice
- [ ] Test anti-spam roll (tidak bisa roll 2x)
- [ ] Test jail actions auto turn
- [ ] Test semua nama kota tampil dengan benar
- [ ] Integration test full game flow
- [ ] Test multiplayer scenario

---

## 🆘 Support

Jika ada pertanyaan atau masalah saat migrasi:

1. Cek dokumentasi API lengkap: `MONOPOLY_API_DOCUMENTATION.md`
2. Test endpoint dengan Swagger UI: `http://localhost:5000/swagger`
3. Test dengan cURL (lihat contoh di dokumentasi utama)

---

## ⚠️ Known Issues & Questions

### **Issue 1: Buy Property Flow Tidak Jelas**

**Problem:** 
- Roll dice langsung auto turn
- Tapi buy property tidak auto turn
- Kapan pemain bisa buy property jika setelah roll dice sudah bukan turn-nya?

**Possible Solutions:**
1. Frontend harus handle race condition (buy property sebelum state refresh)
2. Backend perlu refactor: roll dice tidak auto turn, tambah explicit "end turn" trigger
3. Buy property removed/disabled (pemain tidak bisa beli property)

**Status:** Menunggu klarifikasi

### **Issue 2: AvailableActions Tidak Akurat**

**Problem:**
- `availableActions` di response state mungkin tidak akurat
- Setelah roll dice, available actions mungkin masih tampil tapi sudah tidak valid (karena turn berganti)

**Solution:**
Frontend harus selalu cek `currentPlayerName` sebelum enable action buttons

---

**Last Updated:** Februari 2026  
**Version:** 2.0  
**Status:** ⚠️ Perlu Klarifikasi Flow Buy Property
