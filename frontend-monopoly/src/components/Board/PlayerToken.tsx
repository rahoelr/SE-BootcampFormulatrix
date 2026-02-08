import type { PlayerResponse } from '../../types';
import { playerTokenColors } from '../../utils/propertyColors';

interface PlayerTokenProps {
  player: PlayerResponse;
  playerIndex: number;
  size?: 'sm' | 'md';
}

export function PlayerToken({ player, playerIndex, size = 'sm' }: PlayerTokenProps) {
  const sizeClass = size === 'sm' ? 'w-5 h-5 text-[10px]' : 'w-8 h-8 text-sm';
  const colorClass = playerTokenColors[playerIndex % playerTokenColors.length];

  return (
    <div
      className={`${sizeClass} ${colorClass} flex items-center justify-center text-white font-display font-black shadow-brutal-sm border-2 border-black uppercase`}
      title={player.name}
    >
      {player.name.charAt(0).toUpperCase()}
    </div>
  );
}
