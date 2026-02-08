import { useState, useEffect, useCallback } from 'react';
import { gameApi } from '../services/api';
import type { GameStateResponse, BoardResponse } from '../types';

export function useGameState() {
  const [gameState, setGameState] = useState<GameStateResponse | null>(null);
  const [board, setBoard] = useState<BoardResponse | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Fetch board configuration (static, only once)
  useEffect(() => {
    gameApi
      .getBoard()
      .then((res) => setBoard(res.data))
      .catch((err) => console.error('Failed to load board:', err));
  }, []);

  // Refresh game state
  const refreshState = useCallback(async () => {
    try {
      setLoading(true);
      const res = await gameApi.getState();
      setGameState(res.data);
      setError(null);
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed to fetch game state');
    } finally {
      setLoading(false);
    }
  }, []);

  // Execute action with auto-refresh
  const executeAction = useCallback(
    async <T,>(action: () => Promise<{ data: T }>): Promise<T> => {
      try {
        setLoading(true);
        const result = await action();
        await refreshState(); // Auto-refresh after action
        return result.data;
      } catch (err: any) {
        setError(err.response?.data?.error || 'Action failed');
        throw err;
      } finally {
        setLoading(false);
      }
    },
    [refreshState]
  );

  return {
    gameState,
    board,
    loading,
    error,
    refreshState,
    executeAction,
  };
}
