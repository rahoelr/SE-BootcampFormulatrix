import type { PlayerResponse } from '../../types';
import { PlayerToken } from '../Board/PlayerToken';
import { PropertyList } from './PropertyList';

interface PlayerCardProps {
  player: PlayerResponse;
  playerIndex: number;
  isCurrentTurn: boolean;
}

export function PlayerCard({ player, playerIndex, isCurrentTurn }: PlayerCardProps) {
  const getStateBadge = () => {
    if (player.state === 'Bankrupt') {
      return <span className="text-xs bg-black text-white px-2 py-1 font-display font-bold uppercase border-2 border-black">💀 Bankrupt</span>;
    }
    if (player.state === 'InJail') {
      return (
        <span className="text-xs bg-black text-white px-2 py-1 font-display font-bold uppercase border-2 border-black">
          🔒 In Jail ({player.jailTurns}/3)
        </span>
      );
    }
    return <span className="text-xs bg-white text-black px-2 py-1 font-display font-bold uppercase border-2 border-black">✓ Active</span>;
  };

  return (
    <div
      className={`
        bg-white p-4 border-4 border-black shadow-brutal transition-all duration-100
        ${isCurrentTurn ? 'shadow-brutal-lg border-brutal-yellow' : ''}
      `}
    >
      {/* Header */}
      <div className="flex items-center gap-3 mb-3">
        <PlayerToken player={player} playerIndex={playerIndex} size="md" />
        <div className="flex-1">
          <h3 className={`font-display font-black text-lg uppercase ${isCurrentTurn ? 'text-black' : 'text-black'}`}>
            {player.name}
          </h3>
          {getStateBadge()}
        </div>
      </div>

      {/* Money */}
      <div className="mb-3">
        <div className="text-sm font-display font-bold uppercase tracking-wide">Money</div>
        <div className="text-3xl font-mono font-black text-black">
          ${player.money.toLocaleString()}
        </div>
      </div>

      {/* Position */}
      <div className="mb-3 text-sm">
        <span className="font-display font-bold uppercase">Position:</span>{' '}
        <span className="font-body font-semibold">{player.currentTileName}</span>
        <span className="font-body text-black ml-2">({player.currentTileType})</span>
      </div>

      {/* Properties */}
      <PropertyList properties={player.properties} />

      {/* Get Out of Jail Card */}
      {player.hasGetOutOfJailCard && (
        <div className="mt-3 bg-brutal-yellow border-3 border-black px-2 py-1 text-xs font-display font-bold uppercase shadow-brutal-sm">
          🎫 Has Get Out of Jail Free card
        </div>
      )}
    </div>
  );
}
