import { useState, useEffect } from 'react';
import { gameApi } from '../../services/api';
import toast from 'react-hot-toast';
import diceLogo from '../../assets/dice_logo.png';
import monopolyBg from '../../assets/monopoly_bg.webp';

interface GameSetupProps {
  onGameCreated: () => void;
  onReset?: () => void;
  hasActiveGame?: boolean;
}

export function GameSetup({ onGameCreated, onReset, hasActiveGame }: GameSetupProps) {
  const [numPlayers, setNumPlayers] = useState(2);
  const [playerNames, setPlayerNames] = useState<string[]>(['', '']);
  const [loading, setLoading] = useState(false);
  const [isPageLoading, setIsPageLoading] = useState(true);
  const [loadingProgress, setLoadingProgress] = useState(0);

  // Loading screen effect
  useEffect(() => {
    let currentProgress = 0;
    const interval = setInterval(() => {
      currentProgress += Math.random() * 3 + 1;
      if (currentProgress >= 100) {
        currentProgress = 100;
        setLoadingProgress(100);
        setTimeout(() => {
          setIsPageLoading(false);
        }, 500);
        clearInterval(interval);
      } else {
        setLoadingProgress(Math.floor(currentProgress));
      }
    }, 150);

    return () => clearInterval(interval);
  }, []);

  const getLoadingMessage = () => {
    if (loadingProgress < 20) return 'Loading game assets...';
    if (loadingProgress < 40) return 'Preparing the board...';
    if (loadingProgress < 60) return 'Shuffling Chance cards...';
    if (loadingProgress < 80) return 'Counting money...';
    if (loadingProgress < 95) return 'Setting up properties...';
    return 'Almost ready!';
  };

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

  // Loading Screen
  if (isPageLoading) {
    return (
      <div 
        className="min-h-screen flex flex-col items-center justify-center p-4 bg-cover bg-center bg-no-repeat"
        style={{ backgroundImage: `url(${monopolyBg})` }}
      >
        <div className="bg-white/95 backdrop-blur-sm border-4 border-black shadow-brutal-lg p-8 max-w-md w-full text-center">
          {/* Animated Dice */}
          <div className="flex justify-center gap-4 mb-6">
            <img 
              src={diceLogo} 
              alt="Dice" 
              className="w-16 h-16 animate-bounce" 
              style={{ animationDelay: '0ms' }}
            />
            <img 
              src={diceLogo} 
              alt="Dice" 
              className="w-16 h-16 animate-bounce" 
              style={{ animationDelay: '150ms' }}
            />
            <img 
              src={diceLogo} 
              alt="Dice" 
              className="w-16 h-16 animate-bounce" 
              style={{ animationDelay: '300ms' }}
            />
          </div>

          {/* Title */}
          <h1 className="text-4xl font-display font-black text-black uppercase tracking-tight mb-4">
            Loading Monopoly
          </h1>

          {/* Loading Message */}
          <p className="text-lg font-body text-black mb-4 font-semibold animate-pulse">
            {getLoadingMessage()}
          </p>

          {/* Progress Bar Container */}
          <div className="relative w-full h-8 bg-gray-200 border-4 border-black overflow-hidden shadow-brutal">
            {/* Progress Bar Fill */}
            <div 
              className="h-full transition-all duration-300 ease-out relative overflow-hidden"
              style={{ 
                width: `${loadingProgress}%`,
                background: 'linear-gradient(90deg, #22c55e 0%, #16a34a 50%, #22c55e 100%)',
                backgroundSize: '200% 100%',
                animation: 'shimmer 1.5s infinite'
              }}
            >
              {/* Animated stripes */}
              <div 
                className="absolute inset-0 opacity-30"
                style={{
                  backgroundImage: 'repeating-linear-gradient(45deg, transparent, transparent 10px, rgba(255,255,255,0.5) 10px, rgba(255,255,255,0.5) 20px)',
                  animation: 'moveStripes 0.5s linear infinite'
                }}
              />
            </div>
            
            {/* Percentage Text */}
            <div className="absolute inset-0 flex items-center justify-center">
              <span className="font-display font-black text-black text-lg drop-shadow-sm">
                {loadingProgress}%
              </span>
            </div>
          </div>

          {/* Fun Facts */}
          <div className="mt-6 p-3 bg-brutal-yellow border-3 border-black">
            <p className="text-sm font-body text-black font-semibold">
              💡 Did you know? Monopoly was first published in 1935!
            </p>
          </div>
        </div>

        {/* CSS Animation Keyframes */}
        <style>{`
          @keyframes shimmer {
            0% { background-position: 200% 0; }
          100% { background-position: -200% 0; }
        }
        @keyframes moveStripes {
          0% { transform: translateX(-20px); }
          100% { transform: translateX(0); }
        }
      `}</style>
    </div>
  );
}

  return (
    <div 
      className="min-h-screen flex items-center justify-center p-4 bg-cover bg-center bg-no-repeat"
      style={{ backgroundImage: `url(${monopolyBg})` }}
    >
      <div className="bg-white border-4 border-black shadow-brutal-lg p-8 max-w-md w-full">
        <h1 className="text-5xl font-display font-black text-center mb-2 text-black uppercase tracking-tight flex items-center justify-center gap-3">
          <img src={diceLogo} alt="Dice" className="w-12 h-12 animate-bounce" /> 
          Monopoly
          <img src={diceLogo} alt="Dice" className="w-12 h-12 animate-bounce" />
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
