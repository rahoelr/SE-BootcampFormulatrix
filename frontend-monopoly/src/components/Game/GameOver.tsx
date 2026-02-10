import type { GameStateResponse } from '../../types';
import monopolyBg from '../../assets/monopoly_bg.webp';
import money1 from '../../assets/money1.png';
import money2 from '../../assets/money2.png';

interface GameOverProps {
  gameState: GameStateResponse;
  onNewGame: () => void;
}

export function GameOver({ gameState, onNewGame }: GameOverProps) {
  const winner = gameState.players.find(p => p.name === gameState.winnerName);
  const sortedPlayers = [...gameState.players].sort((a, b) => b.money - a.money);

  // Generate random money image positions - MORE FESTIVE!
  const moneyElements = Array.from({ length: 50 }, (_, i) => ({
    id: i,
    image: i % 2 === 0 ? money1 : money2,
    left: `${Math.random() * 100}%`,
    delay: `${Math.random() * 3}s`, // Faster start
    duration: `${3 + Math.random() * 4}s`, // Faster fall (3-7s)
    size: `${50 + Math.random() * 70}px`, // Bigger range (50-120px)
    rotation: `${Math.random() * 360}deg`, // Random initial rotation
    horizontalMove: `${-50 + Math.random() * 100}px`, // Side-to-side movement
  }));

  return (
    <div 
      className="min-h-screen flex items-center justify-center p-4 bg-cover bg-center bg-no-repeat relative overflow-hidden"
      style={{ backgroundImage: `url(${monopolyBg})` }}
    >
      {/* Money Falling Animation Background */}
      <div className="absolute inset-0 pointer-events-none overflow-hidden">
        {moneyElements.map((money) => (
          <img
            key={money.id}
            src={money.image}
            alt="money"
            className="absolute animate-fall-festive opacity-90 drop-shadow-lg"
            style={{
              left: money.left,
              top: '-15%',
              width: money.size,
              height: 'auto',
              animationDelay: money.delay,
              animationDuration: money.duration,
              '--horizontal-move': money.horizontalMove,
              '--initial-rotation': money.rotation,
              filter: 'drop-shadow(0 0 8px rgba(255, 215, 0, 0.5))', // Gold glow
            } as React.CSSProperties}
          />
        ))}
      </div>

      <style>{`
        @keyframes fall-festive {
          0% {
            transform: translateY(0) translateX(0) rotate(var(--initial-rotation, 0deg)) scale(0.5);
            opacity: 0;
          }
          5% {
            opacity: 0.9;
            transform: translateY(5vh) translateX(0) rotate(var(--initial-rotation, 0deg)) scale(1);
          }
          25% {
            transform: translateY(25vh) translateX(var(--horizontal-move, 0)) rotate(calc(var(--initial-rotation, 0deg) + 90deg)) scale(1.1);
          }
          50% {
            transform: translateY(50vh) translateX(calc(var(--horizontal-move, 0) * -0.5)) rotate(calc(var(--initial-rotation, 0deg) + 180deg)) scale(1);
          }
          75% {
            transform: translateY(75vh) translateX(var(--horizontal-move, 0)) rotate(calc(var(--initial-rotation, 0deg) + 270deg)) scale(1.05);
          }
          95% {
            opacity: 0.9;
          }
          100% {
            transform: translateY(115vh) translateX(0) rotate(calc(var(--initial-rotation, 0deg) + 360deg)) scale(0.8);
            opacity: 0;
          }
        }
        
        .animate-fall-festive {
          animation: fall-festive linear infinite;
        }
        
        @keyframes glow-pulse {
          0%, 100% {
            filter: drop-shadow(0 0 8px rgba(255, 215, 0, 0.5));
          }
          50% {
            filter: drop-shadow(0 0 15px rgba(255, 215, 0, 0.8));
          }
        }
      `}</style>

      <div className="bg-white/95 backdrop-blur-sm border-4 border-black shadow-brutal-lg p-8 max-w-2xl w-full relative z-10">
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
