import type { TileResponse, PlayerResponse, PropertyResponse } from '../../types';
import { PlayerToken } from './PlayerToken';
import { isCornerTile } from '../../utils/boardLayout';

interface TileProps {
  tile: TileResponse;
  playersOnTile: { player: PlayerResponse; index: number }[];
  owner: PropertyResponse | null;
}

export function Tile({ tile, playersOnTile, owner }: TileProps) {
  const corner = isCornerTile(tile.position);

  const getCornerEmoji = () => {
    if (tile.position === 0) return '▶️'; // GO/Start
    if (tile.position === 10) return '👮'; // Jail - Police
    if (tile.position === 20) return '🅿️'; // Free Parking
    if (tile.position === 30) return '👮'; // Go to Jail - Police
    return '';
  };

  const getSpecialEmoji = () => {
    if (tile.effect === 'Chance') return '?';
    if (tile.effect === 'CommunityChest') return '📦';
    if (tile.effect === 'Tax') return '💰';
    return '';
  };

  return (
    <div
      className={`
        relative border-3 border-black bg-white
        h-[90px] p-2 shadow-brutal-tile
        flex flex-col justify-between
        hover:shadow-brutal-sm transition-all duration-100
        overflow-hidden
      `}
    >
      {/* Tile content */}
      <div className="flex-1 flex flex-col justify-between min-h-0">
        {/* Title */}
        <div className="text-center overflow-hidden">
          {corner && (
            <div className="text-2xl mb-1">{getCornerEmoji()}</div>
          )}
          {tile.effect === 'Chance' || tile.effect === 'CommunityChest' || tile.effect === 'Tax' ? (
            <div className="text-xl mb-1">{getSpecialEmoji()}</div>
          ) : null}
          {tile.type === 'Railroad' && <div className="text-xl mb-1">🚂</div>}
          {tile.type === 'Utility' && (
            <div className="text-xl mb-1">
              {tile.name.includes('Electric') || tile.name.includes('PLN') ? '⚡' : '💧'}
            </div>
          )}
          {tile.type === 'Property' && <div className="text-xl mb-1">🏘️</div>}
          <div className={`
            ${corner ? 'text-sm' : tile.type === 'Railroad' || tile.type === 'Utility' ? 'text-sm' : 'text-xs'} 
            font-display font-bold leading-tight uppercase 
            ${tile.type === 'Railroad' || tile.type === 'Utility' ? 'line-clamp-2' : 'truncate'} 
            w-full px-0.5
          `}>
            {tile.name}
          </div>
        </div>

        {/* Price */}
        {tile.price !== null && (
          <div className="text-[10px] text-center text-black font-mono mt-1 font-semibold">
            ${tile.price}
          </div>
        )}

        {/* Owner indicator */}
        {owner && (
          <div className="absolute top-0.5 right-0.5">
            <div className="w-2 h-2 bg-green-500 border border-black" title={`Owned by ${owner.ownerName}`} />
          </div>
        )}

        {/* Houses */}
        {owner && owner.houses > 0 && (
          <div className="absolute top-0.5 left-0.5 text-[9px] leading-none">
            {owner.houses === 5 ? '🏨' : `${'🏠'.repeat(owner.houses)}`}
          </div>
        )}

        {/* Mortgaged */}
        {owner && owner.isMortgaged && (
          <div className="absolute bottom-1 right-1 text-[10px]">🔒</div>
        )}
      </div>

      {/* Players on this tile */}
      {playersOnTile.length > 0 && (
        <div className="flex flex-wrap gap-0.5 mt-1 justify-center overflow-hidden max-h-8">
          {playersOnTile.map(({ player, index }) => (
            <PlayerToken key={player.name} player={player} playerIndex={index} size="sm" />
          ))}
        </div>
      )}
    </div>
  );
}
