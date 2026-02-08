// ============================================
// REQUEST TYPES
// ============================================

export interface CreateGameRequest {
  playerNames: string[]; // 2-4 players
}

export interface PlayerActionRequest {
  playerName: string;
}

export interface PropertyActionRequest extends PlayerActionRequest {
  propertyName: string;
}

export interface TradeRequest extends PlayerActionRequest {
  targetPlayerName: string;
  offeredProperties: string[];
  offeredMoney: number;
  requestedProperties: string[];
  requestedMoney: number;
}

// ============================================
// RESPONSE TYPES
// ============================================

export interface GameStateResponse {
  isGameStarted: boolean;
  isGameOver: boolean;
  winnerName: string | null;
  currentTurn: number;
  currentPlayerName: string;
  availableActions: AvailableAction[];
  players: PlayerResponse[];
  allProperties: PropertyResponse[];
}

export interface PlayerResponse {
  name: string;
  position: number;
  currentTileName: string;
  currentTileType: TileType;
  money: number;
  state: PlayerState;
  properties: PropertyResponse[];
  jailTurns: number;
  hasGetOutOfJailCard: boolean;
}

export interface PropertyResponse {
  name: string;
  type: AssetType;
  value: number;
  ownerName: string | null;
  houses: number; // 0-4 houses, 5 = hotel
  isMortgaged: boolean;
  rent: number;
}

export interface TileResponse {
  position: number;
  name: string;
  type: TileType;
  effect: EffectType;
  price: number | null;
  assetType: AssetType | null;
}

export interface BoardResponse {
  tiles: TileResponse[];
  totalTiles: number;
}

export interface RollDiceResponse {
  dice1: number;
  dice2: number;
  total: number;
  isDouble: boolean;
  newPosition: number;
  landedTile: string;
}

export interface ActionResultResponse {
  success: boolean;
  message: string;
  data?: any;
}

// Force End Game Response
export interface PlayerRankingResponse {
  rank: number;
  playerName: string;
  totalWealth: number;
  cash: number;
  assetsValue: number;
  propertyCount: number;
  houseCount: number;
}

export interface GameResultResponse {
  isGameOver: boolean;
  winnerName: string;
  totalTurns: number;
  rankings: PlayerRankingResponse[];
}

export interface ForceEndGameResponse {
  success: boolean;
  message: string;
  gameResult: GameResultResponse;
}

// ============================================
// ENUMS
// ============================================

export type PlayerState = 'Normal' | 'InJail' | 'Bankrupt';

export type AssetType = 'RealEstate' | 'PublicService' | 'Railroad';

export type TileType = 'Property' | 'Railroad' | 'Utility' | 'Corner' | 'Special';

export type EffectType = 
  | 'Go'
  | 'Nothing'
  | 'CommunityChest'
  | 'Chance'
  | 'Tax'
  | 'GoToJail'
  | 'FreeParking';

export type AvailableAction =
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
  | 'end-turn';
