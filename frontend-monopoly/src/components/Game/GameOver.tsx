import type { GameStateResponse } from '../../types';

interface GameOverProps {
  gameState: GameStateResponse;
  onNewGame: () => void;
}

export function GameOver({ gameState, onNewGame }: GameOverProps) {
  const winner = gameState.players.find(p => p.name === gameState.winnerName);
  const sortedPlayers = [...gameState.players].sort((a, b) => b.money - a.money);

  return (
    <div className="min-h-screen bg-white flex items-center justify-center p-4">
      <div className="bg-white border-4 border-black shadow-brutal-lg p-8 max-w-2xl w-full">
        <div className="text-center mb-8">
          <div className="text-6xl mb-4">🏆</div>
          <h1 className="text-5xl font-display font-black text-black mb-2 uppercase tracking-tight">
            Game Over!
          </h1>
          <h2 className="text-2xl font-display font-bold text-black uppercase">
            Winner: {gameState.winnerName}
          </h2>
          {winner && (
            <p className="text-xl font-mono text-black mt-2 font-bold">
              Final Money: ${winner.money.toLocaleString()}
            </p>
          )}
        </div>

        <div className="mb-8">
          <h3 className="text-xl font-display font-black mb-4 text-center uppercase tracking-wide">Final Standings</h3>
          <table className="w-full border-4 border-black">
            <thead className="bg-brutal-yellow">
              <tr>
                <th className="px-4 py-2 text-left font-display font-bold uppercase border-b-4 border-black">Rank</th>
                <th className="px-4 py-2 text-left font-display font-bold uppercase border-b-4 border-black">Player</th>
                <th className="px-4 py-2 text-right font-display font-bold uppercase border-b-4 border-black">Money</th>
                <th className="px-4 py-2 text-right font-display font-bold uppercase border-b-4 border-black">Properties</th>
                <th className="px-4 py-2 text-left font-display font-bold uppercase border-b-4 border-black">Status</th>
              </tr>
            </thead>
            <tbody className="bg-white">
              {sortedPlayers.map((player, index) => (
                <tr key={player.name} className="border-b-3 border-black">
                  <td className="px-4 py-3 font-display font-bold">
                    {index === 0 ? '🥇' : index === 1 ? '🥈' : index === 2 ? '🥉' : `${index + 1}.`}
                  </td>
                  <td className="px-4 py-3 font-body font-semibold">{player.name}</td>
                  <td className="px-4 py-3 text-right font-mono font-bold">
                    ${player.money.toLocaleString()}
                  </td>
                  <td className="px-4 py-3 text-right font-body font-semibold">{player.properties.length}</td>
                  <td className="px-4 py-3">
                    {player.state === 'Bankrupt' ? (
                      <span className="font-display font-bold uppercase text-xs">💀 Bankrupt</span>
                    ) : (
                      <span className="font-display font-bold uppercase text-xs">✓ Active</span>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        <button
          onClick={onNewGame}
          className="w-full bg-brutal-yellow text-black py-3 font-display font-black text-lg uppercase tracking-wide border-4 border-black shadow-brutal hover:shadow-brutal-sm hover:translate-x-[2px] hover:translate-y-[2px] active:shadow-none active:translate-x-[4px] active:translate-y-[4px] transition-all duration-100"
        >
          Start New Game
        </button>
      </div>
    </div>
  );
}
