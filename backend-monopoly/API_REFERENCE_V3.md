# API Reference V3.0 - Monopoly Backend

Dokumentasi lengkap API untuk Monopoly Backend versi 3.0 dengan sistem Manual End Turn.

---

## 📋 Table of Contents

- [Overview](#overview)
- [Base Information](#base-information)
- [Authentication](#authentication)
- [Endpoints](#endpoints)
  - [Game Management](#game-management)
  - [Player Actions](#player-actions)
  - [Property Management](#property-management)
  - [Jail Actions](#jail-actions)
  - [Turn Management](#turn-management)
- [Data Models](#data-models)
- [Error Handling](#error-handling)
- [Rate Limiting](#rate-limiting)

---

## Overview

### What's New in V3.0

- ✅ **Manual End Turn** - Players must explicitly end their turn
- ✅ **Multiple Actions Per Turn** - Players can perform many actions before ending turn
- ✅ **Indonesian Property Names** - All properties use Indonesian city names
- ✅ **Enhanced Validation** - Stricter rules for game flow

### Breaking Changes from V2.0

- ❌ **Auto Turn Removed** - Roll dice no longer auto-advances turn
- ✅ **End Turn Endpoint Restored** - `POST /api/game/end-turn` is active again

---

## Base Information

### Server Details

| Property | Value |
|----------|-------|
| **Base URL** | `http://localhost:5278/api/game` (development) |
| **Protocol** | HTTP/HTTPS |
| **Content-Type** | `application/json` |
| **Response Format** | JSON |

### CORS Configuration

Allowed origins:
- `http://localhost:3000` (Create React App)
- `http://localhost:5173` (Vite)

---

## Authentication

**Current Version:** No authentication required (stateless game)

**Note:** Game state is stored in server memory. Only one active game at a time.

---

## Endpoints

### Game Management

#### 1. Create Game

Creates a new game with 2-4 players.

**Endpoint:** `POST /api/game/create`

**Request Body:**
```json
{
  "playerNames": ["Alice", "Bob", "Charlie"]
}
```

**Validation:**
- Minimum 2 players
- Maximum 4 players
- Player names must be unique
- Each name must be non-empty string

**Response (200 OK):**
```json
{
  "isGameOver": false,
  "winnerName": null,
  "currentTurn": 0,
  "currentPlayerName": "Alice",
  "players": [
    {
      "name": "Alice",
      "position": 0,
      "currentTileName": "GO",
      "currentTileType": "Corner",
      "money": 1500,
      "state": "Normal",
      "properties": [],
      "jailTurns": 0,
      "hasGetOutOfJailCard": false
    }
  ],
  "allProperties": [
    {
      "name": "Medan",
      "price": 60,
      "type": "RealEstate",
      "isMortgaged": false,
      "houses": 0
    }
  ],
  "availableActions": ["roll-dice"]
}
```

**Error Responses:**

| Code | Scenario | Response |
|------|----------|----------|
| 400 | Invalid player count | `{"error": "Must have 2-4 players"}` |
| 400 | Duplicate names | `{"error": "Player names must be unique"}` |
| 409 | Game exists | `{"error": "Game already exists. Reset first."}` |

---

#### 2. Reset Game

Deletes current game and allows creating a new one.

**Endpoint:** `POST /api/game/reset`

**Request Body:** None

**Response (200 OK):**
```json
{
  "message": "Game reset successfully"
}
```

---

#### 3. Get Game Status

Checks if there's an active game.

**Endpoint:** `GET /api/game/status`

**Response (200 OK):**
```json
{
  "hasActiveGame": true
}
```

---

#### 4. Get Board Configuration

Returns static board configuration (40 tiles).

**Endpoint:** `GET /api/game/board`

**Response (200 OK):**
```json
{
  "tiles": [
    {
      "position": 0,
      "name": "GO",
      "type": "Corner",
      "effect": "Go",
      "price": null,
      "assetType": null
    },
    {
      "position": 1,
      "name": "Medan",
      "type": "Property",
      "effect": "Nothing",
      "price": 60,
      "assetType": "RealEstate"
    }
  ],
  "totalTiles": 40
}
```

---

#### 5. Get Game State

Returns current game state including all players and properties.

**Endpoint:** `GET /api/game/state`

**Response (200 OK):** Same structure as Create Game response

**Error (404):**
```json
{
  "error": "No active game. Create one first."
}
```

---

### Player Actions

#### 6. Roll Dice

Rolls dice and moves player. **Does NOT auto-advance turn in V3.0.**

**Endpoint:** `POST /api/game/roll-dice`

**Request Body:**
```json
{
  "playerName": "Alice"
}
```

**Response (200 OK):**
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

**Behavior (V3.0):**
- ✅ Rolls 2 dice (1-6 each)
- ✅ Moves player
- ✅ Processes tile effects
- ✅ **Turn remains the same** (no auto turn)
- ✅ Player can perform more actions
- ✅ Sets `_hasRolledThisTurn` flag

**Validation:**
- Must be player's turn
- Player must not be in jail
- Player can only roll once per turn

**Error Responses:**

| Scenario | Error Message |
|----------|---------------|
| Not player's turn | `"It's not Alice's turn. Current player is Bob."` |
| Already rolled | `"You have already rolled this turn."` |
| Player in jail | `"Player is in jail. Use jail-specific actions."` |

---

### Property Management

#### 7. Buy Property

Buys property at current position. **Does NOT auto-advance turn.**

**Endpoint:** `POST /api/game/buy-property`

**Request Body:**
```json
{
  "playerName": "Alice"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Successfully bought Medan",
  "data": null
}
```

**Behavior (V3.0):**
- ✅ Buys property at current position
- ✅ Deducts money from player
- ✅ **Turn remains the same** (no auto turn)
- ✅ Player can perform more actions

**Validation:**
- Must be player's turn
- Must be on a property tile
- Property must be available (no owner)
- Player must have enough money

**Error Responses:**

| Scenario | Error Message |
|----------|---------------|
| Not on property | `"This tile has no property to buy."` |
| Already owned | `"Property is already owned."` |
| Insufficient funds | `"Player tidak punya cukup uang untuk membeli [Property]."` |

---

#### 8. Build House

Builds a house on owned property.

**Endpoint:** `POST /api/game/build-house`

**Request Body:**
```json
{
  "playerName": "Alice",
  "propertyName": "Medan"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Built house on Medan",
  "data": null
}
```

**Rules:**
- Must own the property
- Maximum 4 houses (5th = hotel)
- Cost: 50% of property price
- Cannot build on mortgaged property

---

#### 9. Sell House

Sells a house from owned property.

**Endpoint:** `POST /api/game/sell-house`

**Request Body:**
```json
{
  "playerName": "Alice",
  "propertyName": "Medan"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Sold house on Medan",
  "data": null
}
```

**Rules:**
- Must own the property
- Property must have at least 1 house
- Sell price: 25% of property price

---

#### 10. Mortgage Property

Mortgages a property for cash.

**Endpoint:** `POST /api/game/mortgage`

**Request Body:**
```json
{
  "playerName": "Alice",
  "propertyName": "Medan"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Mortgaged Medan",
  "data": null
}
```

**Rules:**
- Must own the property
- Property must not already be mortgaged
- Must sell all houses first
- Mortgage value: 50% of property price

---

#### 11. Unmortgage Property

Redeems a mortgaged property.

**Endpoint:** `POST /api/game/unmortgage`

**Request Body:**
```json
{
  "playerName": "Alice",
  "propertyName": "Medan"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Unmortgaged Medan",
  "data": null
}
```

**Rules:**
- Must own the property
- Property must be mortgaged
- Cost: 110% of mortgage value (10% interest)

---

#### 12. Trade

Trades properties and money with another player.

**Endpoint:** `POST /api/game/trade`

**Request Body:**
```json
{
  "playerName": "Alice",
  "targetPlayerName": "Bob",
  "offeredProperties": ["Medan"],
  "offeredMoney": 50,
  "requestedProperties": ["Palembang"],
  "requestedMoney": 0
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Trade completed successfully",
  "data": null
}
```

**Rules:**
- Both players must exist
- Properties must be owned by respective players
- Both players must have sufficient money
- Cannot trade mortgaged properties

---

### Jail Actions

#### 13. Pay Jail Fee

Pays $50 to get out of jail. **Does NOT auto-advance turn in V3.0.**

**Endpoint:** `POST /api/game/pay-jail-fee`

**Request Body:**
```json
{
  "playerName": "Alice"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Paid jail fee and released",
  "data": null
}
```

**Behavior (V3.0):**
- ✅ Deducts $50 from player
- ✅ Releases player from jail
- ✅ **Turn remains the same** (no auto turn)
- ✅ Player must manually end turn

**Validation:**
- Must be player's turn
- Player must be in jail
- Player must have at least $50

---

#### 14. Use Get Out of Jail Card

Uses "Get Out of Jail Free" card. **Does NOT auto-advance turn in V3.0.**

**Endpoint:** `POST /api/game/use-jail-card`

**Request Body:**
```json
{
  "playerName": "Alice"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Used Get Out of Jail card",
  "data": null
}
```

**Behavior (V3.0):**
- ✅ Uses one jail card
- ✅ Releases player from jail
- ✅ **Turn remains the same** (no auto turn)
- ✅ Player must manually end turn

**Validation:**
- Must be player's turn
- Player must be in jail
- Player must have at least 1 jail card

---

#### 15. Try Roll Doubles

Attempts to roll doubles to get out of jail. **Does NOT auto-advance turn in V3.0.**

**Endpoint:** `POST /api/game/try-roll-doubles`

**Request Body:**
```json
{
  "playerName": "Alice"
}
```

**Response (200 OK):**
```json
{
  "dice1": 3,
  "dice2": 3,
  "total": 6,
  "isDouble": true,
  "newPosition": 16,
  "landedTile": "Denpasar"
}
```

**Behavior (V3.0):**
- ✅ Rolls 2 dice
- ✅ If doubles: released and moved
- ✅ If not doubles: stays in jail
- ✅ **Turn remains the same** (no auto turn)
- ✅ Player must manually end turn

**Rules:**
- After 3 failed attempts, must pay fee
- If doubles rolled, player is released and moved
- Position updated if released

---

### Turn Management

#### 16. End Turn ✅ (NEW/RESTORED in V3.0)

Manually ends current player's turn and advances to next player.

**Endpoint:** `POST /api/game/end-turn`

**Request Body:**
```json
{
  "playerName": "Alice"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Turn ended. Now it's Bob's turn.",
  "data": null
}
```

**Behavior:**
- ✅ Validates player has rolled dice
- ✅ Advances to next player
- ✅ Resets `_hasRolledThisTurn` for next player
- ✅ Skips bankrupt players

**Validation:**
- Must be player's turn
- Player must have rolled dice in this turn

**Error Responses:**

| Scenario | Error Message |
|----------|---------------|
| Not player's turn | `"It's not Alice's turn. Current player is Bob."` |
| Haven't rolled | `"You must roll dice before ending turn."` |

---

## Data Models

### GameStateResponse

```typescript
interface GameStateResponse {
  isGameOver: boolean;
  winnerName: string | null;
  currentTurn: number;
  currentPlayerName: string;
  players: PlayerResponse[];
  allProperties: PropertyResponse[];
  availableActions: string[];
}
```

### PlayerResponse

```typescript
interface PlayerResponse {
  name: string;
  position: number;              // 0-39
  currentTileName: string;
  currentTileType: string;       // "Corner" | "Property" | etc
  money: number;
  state: string;                 // "Normal" | "InJail" | "Bankrupt"
  properties: PropertyResponse[];
  jailTurns: number;
  hasGetOutOfJailCard: boolean;
}
```

### PropertyResponse

```typescript
interface PropertyResponse {
  name: string;
  price: number;
  type: string;                  // "RealEstate" | "Railroad" | "PublicService"
  isMortgaged: boolean;
  houses: number;                // 0-4 houses, 5 = hotel
}
```

### RollDiceResponse

```typescript
interface RollDiceResponse {
  dice1: number;                 // 1-6
  dice2: number;                 // 1-6
  total: number;                 // 2-12
  isDouble: boolean;
  newPosition: number;           // 0-39
  landedTile: string;
}
```

### ActionResultResponse

```typescript
interface ActionResultResponse {
  success: boolean;
  message: string;
  data: any | null;
}
```

---

## Error Handling

### Error Response Format

```json
{
  "error": "Error message here"
}
```

### HTTP Status Codes

| Code | Meaning | When |
|------|---------|------|
| 200 | OK | Request successful |
| 400 | Bad Request | Validation error, invalid input |
| 404 | Not Found | No active game, endpoint not found |
| 409 | Conflict | Game already exists |
| 500 | Internal Server Error | Server error |

### Common Error Messages

| Error | Cause | Solution |
|-------|-------|----------|
| `"No active game. Create one first."` | Trying to access game state without creating game | Call `POST /create` first |
| `"It's not [Player]'s turn."` | Player trying to act out of turn | Wait for your turn |
| `"You have already rolled this turn."` | Trying to roll dice twice | End turn to roll again |
| `"You must roll dice before ending turn."` | Trying to end turn without rolling | Roll dice first |
| `"Game already exists. Reset first."` | Trying to create new game while one exists | Call `POST /reset` first |

---

## Rate Limiting

**Current Version:** No rate limiting

**Future Consideration:** May implement rate limiting in production version.

---

## Best Practices

### 1. Always Check Game State

```typescript
// Get current state before actions
const state = await gameApi.getState();
if (state.currentPlayerName === myPlayerName) {
  // It's my turn
}
```

### 2. Handle All Error Cases

```typescript
try {
  await gameApi.rollDice(playerName);
} catch (error) {
  if (error.response?.status === 400) {
    // Handle validation error
    console.log(error.response.data.error);
  }
}
```

### 3. Follow Turn Flow

```typescript
// Correct flow
await gameApi.rollDice(currentPlayer);        // 1. Roll
await gameApi.buyProperty(currentPlayer);     // 2. Buy (optional)
await gameApi.buildHouse({ ... });            // 3. Build (optional)
await gameApi.endTurn(currentPlayer);         // 4. End turn (mandatory)
```

### 4. Use Available Actions

```typescript
const { availableActions } = await gameApi.getState();

if (availableActions.includes('roll-dice')) {
  // Show roll dice button
}

if (availableActions.includes('end-turn')) {
  // Show end turn button
}
```

---

## Examples

### Complete Turn Example

```typescript
// 1. Check it's my turn
const state = await gameApi.getState();
if (state.currentPlayerName !== myPlayerName) {
  return; // Not my turn
}

// 2. Roll dice
const rollResult = await gameApi.rollDice(myPlayerName);
console.log(`Rolled ${rollResult.total}, landed on ${rollResult.landedTile}`);

// 3. Buy property if available
if (canBuyProperty(rollResult.landedTile)) {
  await gameApi.buyProperty(myPlayerName);
}

// 4. Build houses on owned properties
for (const property of myProperties) {
  if (shouldBuildHouse(property)) {
    await gameApi.buildHouse({
      playerName: myPlayerName,
      propertyName: property.name
    });
  }
}

// 5. End turn
await gameApi.endTurn(myPlayerName);

// 6. Refresh state
const newState = await gameApi.getState();
console.log(`Now it's ${newState.currentPlayerName}'s turn`);
```

### Jail Handling Example

```typescript
const state = await gameApi.getState();
const me = state.players.find(p => p.name === myPlayerName);

if (me.state === 'InJail') {
  if (me.hasGetOutOfJailCard) {
    // Use card
    await gameApi.useJailCard(myPlayerName);
  } else if (me.money >= 50) {
    // Pay fee
    await gameApi.payJailFee(myPlayerName);
  } else {
    // Try roll doubles
    await gameApi.tryRollDoubles(myPlayerName);
  }
  
  // Must end turn manually
  await gameApi.endTurn(myPlayerName);
}
```

---

## Versioning

### Current Version: 3.0.0

**Semantic Versioning:**
- **Major (3)**: Breaking changes (manual end turn)
- **Minor (0)**: New features (backwards compatible)
- **Patch (0)**: Bug fixes

### Version History

- **3.0.0**: Manual end turn system, Indonesian property names
- **2.0.0**: Auto turn system (deprecated)
- **1.0.0**: Initial release

---

## Support & Contact

For API support:
- Documentation: See `FRONTEND_MIGRATION_GUIDE_V3.md`
- Changelog: See `CHANGELOG.md`
- Full docs: See `MONOPOLY_API_DOCUMENTATION.md`

---

**Last Updated:** Februari 2026  
**Version:** 3.0.0  
**Status:** ✅ Stable
