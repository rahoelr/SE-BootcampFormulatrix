import { useState } from 'react';
import type { GameStateResponse } from '../../types';

interface ForceGameOverModalProps {
  gameState: GameStateResponse;
  onConfirm: (winnerName: string) => void;
  onClose: () => void;
}

export function ForceGameOverModal({ gameState, onConfirm, onClose }: ForceGameOverModalProps) {
  const [selectedWinner, setSelectedWinner] = useState<string>(gameState.players[0]?.name || '');

  const handleConfirm = () => {
    if (selectedWinner) {
      onConfirm(selectedWinner);
    }
  };

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
      <div className="bg-white border-4 border-black shadow-brutal-lg p-6 max-w-md w-full">
        <h2 className="text-2xl font-display font-black text-black mb-4 uppercase tracking-tight text-center">
          💀 Force Game Over
        </h2>
        
        <p className="font-body text-black mb-4 text-center">
          This will end the game immediately. Select a winner:
        </p>

        <div className="space-y-2 mb-6">
          {gameState.players.map((player) => (
            <button
              key={player.name}
              onClick={() => setSelectedWinner(player.name)}
              className={`
                w-full p-3 font-display font-bold uppercase border-4 border-black
                ${selectedWinner === player.name 
                  ? 'bg-brutal-yellow shadow-brutal-sm' 
                  : 'bg-white shadow-brutal hover:shadow-brutal-sm'
                }
                hover:translate-x-[2px] hover:translate-y-[2px]
                transition-all duration-100
              `}
            >
              <div className="flex justify-between items-center">
                <span>{player.name}</span>
                <span className="font-mono text-sm">${player.money.toLocaleString()}</span>
              </div>
            </button>
          ))}
        </div>

        <div className="flex gap-2">
          <button
            onClick={onClose}
            className="flex-1 bg-white text-black px-4 py-3 font-display font-bold uppercase tracking-wide border-4 border-black shadow-brutal hover:shadow-brutal-sm hover:translate-x-[2px] hover:translate-y-[2px] active:shadow-none active:translate-x-[4px] active:translate-y-[4px] transition-all duration-100"
          >
            Cancel
          </button>
          <button
            onClick={handleConfirm}
            className="flex-1 bg-red-500 text-white px-4 py-3 font-display font-bold uppercase tracking-wide border-4 border-black shadow-brutal hover:shadow-brutal-sm hover:translate-x-[2px] hover:translate-y-[2px] active:shadow-none active:translate-x-[4px] active:translate-y-[4px] transition-all duration-100"
          >
            End Game
          </button>
        </div>

        <p className="text-xs font-body text-gray-600 mt-4 text-center">
          ⚠️ This is a test feature for demo purposes
        </p>
      </div>
    </div>
  );
}
