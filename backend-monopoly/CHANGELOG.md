# Changelog - Monopoly Backend API

All notable changes to this project will be documented in this file.

---

## [Version 3.0.0] - Februari 2026

### 🔄 Major Changes: Manual End Turn System

#### **Breaking Changes**

##### ❌ Removed: Auto Turn System
- Roll dice **NO LONGER** automatically advances turn
- Jail actions **NO LONGER** automatically advance turn
- Players can now perform multiple actions per turn

##### ✅ Added: Manual End Turn
- Endpoint `POST /api/game/end-turn` is **ACTIVE** again
- Players **MUST** manually end their turn
- New validation: must roll dice before ending turn

---

### 🏙️ Feature Updates

#### **Property Names - Indonesian Cities**

All properties now use Indonesian city names:

**Real Estate Properties (22):**
- Brown Group: Medan ($60), Palembang ($60)
- Light Blue: Semarang ($100), Surabaya ($100), Makassar ($120)
- Pink: Bandung ($140), Yogyakarta ($140), Solo ($160)
- Orange: Denpasar ($180), Malang ($180), Balikpapan ($200)
- Red: Manado ($220), Pontianak ($220), Batam ($240)
- Yellow: Depok ($260), Tangerang ($260), Bekasi ($280)
- Green: Bogor ($300), Jakarta Selatan ($300), Jakarta Pusat ($320)
- Dark Blue: Jakarta Utara ($350), Jakarta Barat ($400)

**Railroads (4):**
- Stasiun Gambir ($200)
- Stasiun Pasar Senen ($200)
- Stasiun Manggarai ($200)
- Stasiun Tanah Abang ($200)

**Utilities (2):**
- PLN - Perusahaan Listrik Negara ($150)
- PDAM - Perusahaan Daerah Air Minum ($150)

---

### ✨ New Features

#### **1. Manual Turn Control**
- Players can perform multiple actions in one turn
- Actions available after rolling dice:
  - Buy property
  - Build/sell houses
  - Mortgage/unmortgage properties
  - Trade with other players

#### **2. Enhanced Validation**
- Anti-spam: Players can only roll dice once per turn
- End turn validation: Must roll dice before ending turn
- Turn validation: Only current player can perform actions

#### **3. Improved Gameplay Flow**
```
Turn Structure:
1. Roll Dice (mandatory, 1x only)
2. Perform Actions (optional, unlimited)
3. End Turn (mandatory, manual)
```

---

### 🔧 Technical Changes

#### **Modified Files**

**1. Services/GameService.cs**
- Removed `NextTurn()` from `ExecuteRollDice()`
- Removed `NextTurn()` from `ExecutePayJailFee()`
- Removed `NextTurn()` from `ExecuteUseJailCard()`
- Removed `NextTurn()` from `ExecuteTryRollDoublesInJail()`
- **Added** `ExecuteEndTurn()` with roll dice validation
- Modified property names in `CreateSimpleBoard()`
- Modified asset names in `CreateTileAssetMapping()`

**2. Controllers/GameController.cs**
- **Added** endpoint `POST /api/game/end-turn`

**3. Documentation**
- Created `FRONTEND_MIGRATION_GUIDE_V3.md`
- Updated this `CHANGELOG.md`

---

### 📝 API Changes

#### **Modified Endpoints**

##### `POST /api/game/roll-dice`
**Before:**
- Rolls dice
- Moves player
- **Auto advances turn** ❌

**After:**
- Rolls dice
- Moves player
- **Turn remains the same** ✅
- Player can perform more actions

---

##### `POST /api/game/pay-jail-fee`
**Before:**
- Pays jail fee
- **Auto advances turn** ❌

**After:**
- Pays jail fee
- **Turn remains the same** ✅
- Player must manually end turn

---

##### `POST /api/game/use-jail-card`
**Before:**
- Uses get out of jail card
- **Auto advances turn** ❌

**After:**
- Uses get out of jail card
- **Turn remains the same** ✅
- Player must manually end turn

---

##### `POST /api/game/try-roll-doubles`
**Before:**
- Tries to roll doubles in jail
- **Auto advances turn** ❌

**After:**
- Tries to roll doubles in jail
- **Turn remains the same** ✅
- Player must manually end turn

---

#### **New/Restored Endpoints**

##### `POST /api/game/end-turn` ✅
**Status:** ACTIVE

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

**Response (Error - Not Rolled):**
```json
{
  "error": "You must roll dice before ending turn."
}
```

**Validation:**
- Must be current player's turn
- Must have rolled dice in this turn
- Cannot end turn twice

---

### 🎮 Gameplay Changes

#### **Turn Flow Comparison**

**Version 2.0 (Auto Turn):**
```
Player1 → Roll Dice → [AUTO TURN] → Player2
         ↓
    (Cannot buy property - turn already changed)
```

**Version 3.0 (Manual Turn):**
```
Player1 → Roll Dice → Buy Property → Build House → End Turn → Player2
         ↓            ↓               ↓              ↓
    (Turn same)  (Turn same)    (Turn same)    (Turn changes)
```

---

### ⚠️ Migration Required

#### **Frontend Changes Needed:**

1. **Add back** `endTurn()` API call
2. **Add back** "End Turn" button in UI
3. **Remove** assumption that turn auto-advances after roll
4. **Update** game flow to allow multiple actions per turn
5. **Implement** validation for end turn button (enable only after roll)

#### **Code Example:**

```typescript
// Before (V2.0)
await gameApi.rollDice(currentPlayer);
// Turn already changed, cannot do more actions

// After (V3.0)
await gameApi.rollDice(currentPlayer);
// Turn still the same

await gameApi.buyProperty(currentPlayer);
await gameApi.buildHouse({ playerName, propertyName });
// Can perform multiple actions

await gameApi.endTurn(currentPlayer);
// Now turn changes
```

---

### 🧪 Testing

#### **Test Cases Added:**

1. ✅ Roll dice does not auto-advance turn
2. ✅ Anti-spam: Cannot roll dice twice in one turn
3. ✅ Can buy property after rolling dice
4. ✅ Can build house after rolling dice
5. ✅ Must roll dice before ending turn
6. ✅ Manual end turn successfully advances turn
7. ✅ Jail actions do not auto-advance turn
8. ✅ Property names are Indonesian cities

All tests **PASSED** ✅

---

### 📚 Documentation

New documentation files:
- `FRONTEND_MIGRATION_GUIDE_V3.md` - Complete migration guide for frontend
- `CHANGELOG.md` - This file
- `API_REFERENCE_V3.md` - Detailed API reference (planned)

---

### 🐛 Bug Fixes

- Fixed issue where players couldn't buy property after landing on it
- Fixed turn flow that was too restrictive
- Improved gameplay flexibility

---

### ⚡ Performance

No performance impact. All changes are logic-based.

---

### 🔒 Security

No security changes in this version.

---

## [Version 2.0.0] - Februari 2026 (Deprecated)

### Auto Turn System (ROLLED BACK)

#### Changes (No longer active):
- ❌ Auto turn after roll dice (removed in V3.0)
- ❌ Auto turn after jail actions (removed in V3.0)
- ❌ Removed endpoint `/api/game/end-turn` (restored in V3.0)

#### Property Names:
- ✅ Changed to Indonesian cities (retained in V3.0)

---

## [Version 1.0.0] - Original Release

### Features:
- Basic Monopoly game mechanics
- Player management
- Property buying/selling
- House building
- Jail system
- Trading system
- Manual end turn (restored in V3.0)
- American property names (changed in V2.0)

---

## Upgrade Path

### From V1.0 to V3.0:
1. Property names changed to Indonesian cities
2. Manual end turn system (same as V1.0)
3. Enhanced validation (new in V3.0)

### From V2.0 to V3.0:
1. **Add back** manual end turn implementation
2. **Remove** auto turn assumptions
3. **Update** frontend to handle multiple actions per turn

---

## Compatibility

| Version | Frontend Compatibility | Breaking Changes |
|---------|----------------------|------------------|
| V3.0 | Requires update | ⚠️ Yes - Manual end turn |
| V2.0 | Not compatible | ⚠️ Deprecated |
| V1.0 | Not compatible | Property names changed |

---

## Support

For migration help, see:
- `FRONTEND_MIGRATION_GUIDE_V3.md`
- `MONOPOLY_API_DOCUMENTATION.md`

For issues or questions, contact the development team.

---

**Last Updated:** Februari 2026  
**Current Version:** 3.0.0  
**Status:** ✅ Stable
