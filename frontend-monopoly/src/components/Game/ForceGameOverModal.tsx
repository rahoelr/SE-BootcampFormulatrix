import type { GameStateResponse } from '../../types';

interface ForceGameOverModalProps {
  gameState: GameStateResponse;
  onConfirm: () => void;
  onClose: () => void;
}

export function ForceGameOverModal({ gameState, onConfirm, onClose }: ForceGameOverModalProps) {
  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
      <div className="bg-white border-4 border-black shadow-brutal-lg p-6 max-w-md w-full">
        <h2 className="text-2xl font-display font-black text-black mb-4 uppercase tracking-tight text-center">
          💀 Force End Game
        </h2>
        
        <p className="font-body text-black mb-4 text-center">
          This will end the game immediately and calculate the winner based on total wealth.
        </p>

        <div className="bg-brutal-yellow border-4 border-black p-3 mb-4">
          <p className="font-display font-bold text-sm uppercase">Current Players:</p>
          <ul className="font-body text-sm mt-2 space-y-1">
            {gameState.players.map((player) => (
              <li key={player.name}>
                {player.name}: ${player.money.toLocaleString()}
              </li>
            ))}
          </ul>
        </div>

        <div className="flex gap-2">
          <button
            onClick={onClose}
            className="flex-1 bg-white text-black px-4 py-3 font-display font-bold uppercase tracking-wide border-4 border-black shadow-brutal hover:shadow-brutal-sm hover:translate-x-[2px] hover:translate-y-[2px] active:shadow-none active:translate-x-[4px] active:translate-y-[4px] transition-all duration-100"
          >
            Cancel
          </button>
          <button
            onClick={onConfirm}
            className="flex-1 bg-red-500 text-white px-4 py-3 font-display font-bold uppercase tracking-wide border-4 border-black shadow-brutal hover:shadow-brutal-sm hover:translate-x-[2px] hover:translate-y-[2px] active:shadow-none active:translate-x-[4px] active:translate-y-[4px] transition-all duration-100"
          >
            End Game
          </button>
        </div>

        <p className="text-xs font-body text-gray-600 mt-4 text-center">
          ⚠️ Winner will be determined by total wealth (cash + assets)
        </p>
      </div>
    </div>
  );
}
