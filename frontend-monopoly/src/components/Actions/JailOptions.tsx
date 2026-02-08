import type { PlayerResponse } from '../../types';

interface JailOptionsProps {
  player: PlayerResponse;
  onPayFee: () => void;
  onUseCard: () => void;
  onTryDoubles: () => void;
  loading: boolean;
}

export function JailOptions({ player, onPayFee, onUseCard, onTryDoubles, loading }: JailOptionsProps) {
  if (player.state !== 'InJail') return null;

  const canPayFee = player.money >= 50;

  return (
    <div className="bg-orange-50 border-2 border-orange-300 rounded-lg p-4">
      <div className="text-center mb-3">
        <div className="text-2xl mb-2">🔒</div>
        <h3 className="font-bold text-lg text-orange-800">You are in Jail!</h3>
        <p className="text-sm text-orange-700">Turn {player.jailTurns} of 3</p>
      </div>

      <div className="space-y-2">
        <button
          onClick={onPayFee}
          disabled={loading || !canPayFee}
          className="w-full bg-green-500 text-white px-4 py-2 rounded font-medium hover:bg-green-600 disabled:opacity-50 disabled:cursor-not-allowed transition"
        >
          💵 Pay $50 Fee
        </button>

        {player.hasGetOutOfJailCard && (
          <button
            onClick={onUseCard}
            disabled={loading}
            className="w-full bg-yellow-500 text-white px-4 py-2 rounded font-medium hover:bg-yellow-600 disabled:opacity-50 disabled:cursor-not-allowed transition"
          >
            🎫 Use Get Out of Jail Card
          </button>
        )}

        <button
          onClick={onTryDoubles}
          disabled={loading}
          className="w-full bg-blue-500 text-white px-4 py-2 rounded font-medium hover:bg-blue-600 disabled:opacity-50 disabled:cursor-not-allowed transition"
        >
          🎲 Try to Roll Doubles
        </button>
      </div>

      <p className="text-xs text-gray-600 text-center mt-3">
        {player.jailTurns === 3 ? 'Must pay fee or use card this turn!' : 'Choose an option to get out of jail'}
      </p>
    </div>
  );
}
