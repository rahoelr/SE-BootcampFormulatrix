import { useState } from 'react';
import { gameApi } from '../../services/api';
import toast from 'react-hot-toast';

interface GameSetupProps {
  onGameCreated: () => void;
  onReset?: () => void;
  hasActiveGame?: boolean;
}

export function GameSetup({ onGameCreated, onReset, hasActiveGame }: GameSetupProps) {
  const [numPlayers, setNumPlayers] = useState(2);
  const [playerNames, setPlayerNames] = useState<string[]>(['', '']);
  const [loading, setLoading] = useState(false);

  const updatePlayerName = (index: number, name: string) => {
    const newNames = [...playerNames];
    newNames[index] = name;
    setPlayerNames(newNames);
  };

  const handleNumPlayersChange = (num: number) => {
    setNumPlayers(num);
    const newNames = Array(num).fill('').map((_, i) => playerNames[i] || '');
    setPlayerNames(newNames);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    // Validation
    const trimmedNames = playerNames.map(n => n.trim());
    
    if (trimmedNames.some(name => name === '')) {
      toast.error('All player names are required!');
      return;
    }

    const uniqueNames = new Set(trimmedNames);
    if (uniqueNames.size !== trimmedNames.length) {
      toast.error('Player names must be unique!');
      return;
    }

    try {
      setLoading(true);
      await gameApi.createGame({ playerNames: trimmedNames });
      toast.success('Game created successfully!');
      onGameCreated();
    } catch (err) {
      // Error already handled by interceptor
    } finally {
      setLoading(false);
    }
  };

  const handleReset = async () => {
    if (!window.confirm('Are you sure you want to reset the current game?')) {
      return;
    }

    try {
      setLoading(true);
      await gameApi.resetGame();
      toast.success('Game reset successfully!');
      if (onReset) onReset();
    } catch (err) {
      // Error already handled by interceptor
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-white flex items-center justify-center p-4">
      <div className="bg-white border-4 border-black shadow-brutal-lg p-8 max-w-md w-full">
        <h1 className="text-5xl font-display font-black text-center mb-2 text-black uppercase tracking-tight">
          🎲 Monopoly
        </h1>
        <p className="text-center font-body text-black mb-6 uppercase tracking-wide font-semibold">Create a new game</p>

        {hasActiveGame && (
          <div className="bg-brutal-yellow border-4 border-black shadow-brutal p-4 mb-4">
            <p className="text-sm text-black font-body font-semibold">
              A game is already in progress. Reset to start a new one.
            </p>
            <button
              onClick={handleReset}
              disabled={loading}
              className="mt-2 w-full bg-black text-white px-4 py-2 font-display font-bold uppercase tracking-wide border-4 border-black shadow-brutal hover:shadow-brutal-sm hover:translate-x-[2px] hover:translate-y-[2px] active:shadow-none active:translate-x-[4px] active:translate-y-[4px] transition-all duration-100 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {loading ? 'Resetting...' : 'Reset Game'}
            </button>
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-sm font-display font-bold text-black mb-2 uppercase tracking-wide">
              Number of Players
            </label>
            <select
              value={numPlayers}
              onChange={(e) => handleNumPlayersChange(Number(e.target.value))}
              className="w-full border-3 border-black px-4 py-2 font-body shadow-brutal focus:shadow-brutal-sm focus:translate-x-[2px] focus:translate-y-[2px] transition-all duration-100"
              disabled={loading}
            >
              <option value={2}>2 Players</option>
              <option value={3}>3 Players</option>
              <option value={4}>4 Players</option>
            </select>
          </div>

          <div className="space-y-3">
            {Array.from({ length: numPlayers }).map((_, i) => (
              <div key={i}>
                <label className="block text-sm font-display font-bold text-black mb-1 uppercase tracking-wide">
                  Player {i + 1} Name
                </label>
                <input
                  type="text"
                  value={playerNames[i] || ''}
                  onChange={(e) => updatePlayerName(i, e.target.value)}
                  placeholder={`Enter Player ${i + 1} name`}
                  className="w-full border-3 border-black px-4 py-2 font-body shadow-brutal focus:shadow-brutal-sm focus:translate-x-[2px] focus:translate-y-[2px] transition-all duration-100"
                  disabled={loading}
                  required
                />
              </div>
            ))}
          </div>

          <button
            type="submit"
            disabled={loading || hasActiveGame}
            className="w-full bg-brutal-yellow text-black py-3 font-display font-black text-lg uppercase tracking-wide border-4 border-black shadow-brutal hover:shadow-brutal-sm hover:translate-x-[2px] hover:translate-y-[2px] active:shadow-none active:translate-x-[4px] active:translate-y-[4px] transition-all duration-100 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {loading ? 'Creating...' : 'Start Game'}
          </button>
        </form>

        <div className="mt-6 text-center text-sm font-body text-black font-semibold uppercase">
          <p>Each player starts with $1500</p>
          <p>Pass GO to collect $200</p>
        </div>
      </div>
    </div>
  );
}
