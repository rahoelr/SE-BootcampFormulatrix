import axios from 'axios';
import toast from 'react-hot-toast';
import type {
  CreateGameRequest,
  PropertyActionRequest,
  TradeRequest,
  GameStateResponse,
  BoardResponse,
  RollDiceResponse,
  ActionResultResponse,
  ForceEndGameResponse,
} from '../types';

const BASE_URL = 'http://localhost:5278/api/game';  
// const BASE_URL = 'http://13.212.217.150:8080/api/game';

const api = axios.create({
  baseURL: BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

api.interceptors.response.use(
  (response) => response,
  (error) => {
    const message = error.response?.data?.error || 'An error occurred';
    toast.error(message);
    return Promise.reject(error);
  }
);

export const gameApi = {

  createGame: (data: CreateGameRequest) =>
    api.post<GameStateResponse>('/create', data),
  
  resetGame: () =>
    api.post<{ message: string }>('/reset'),
  
  getStatus: () =>
    api.get<{ hasActiveGame: boolean }>('/status'),
  
  getBoard: () =>
    api.get<BoardResponse>('/board'),
  
  getState: () =>
    api.get<GameStateResponse>('/state'),

  rollDice: (playerName: string) =>
    api.post<RollDiceResponse>('/roll-dice', { playerName }),
  
  buyProperty: (playerName: string) =>
    api.post<ActionResultResponse>('/buy-property', { playerName }),
  
  buildHouse: (data: PropertyActionRequest) =>
    api.post<ActionResultResponse>('/build-house', data),
  
  sellHouse: (data: PropertyActionRequest) =>
    api.post<ActionResultResponse>('/sell-house', data),
  
  mortgage: (data: PropertyActionRequest) =>
    api.post<ActionResultResponse>('/mortgage', data),
  
  unmortgage: (data: PropertyActionRequest) =>
    api.post<ActionResultResponse>('/unmortgage', data),
  
  trade: (data: TradeRequest) =>
    api.post<ActionResultResponse>('/trade', data),

  payJailFee: (playerName: string) =>
    api.post<ActionResultResponse>('/pay-jail-fee', { playerName }),
  
  useJailCard: (playerName: string) =>
    api.post<ActionResultResponse>('/use-jail-card', { playerName }),
  
  tryRollDoubles: (playerName: string) =>
    api.post<RollDiceResponse>('/try-roll-doubles', { playerName }),
  
  endTurn: (playerName: string) =>
    api.post<ActionResultResponse>('/end-turn', { playerName }),

  forceEndGame: () =>
    api.post<ForceEndGameResponse>('/force-end'),
};
