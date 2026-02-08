import type { BoardResponse, GameStateResponse } from '../../types';
import { Tile } from './Tile';
import { getTileGridPosition } from '../../utils/boardLayout';
import { TurnIndicator } from '../Game/TurnIndicator';
import { DiceDisplay } from '../Actions/DiceDisplay';
import type { RollDiceResponse } from '../../types';

interface BoardProps {
  board: BoardResponse;
  gameState: GameStateResponse;
  lastRoll: RollDiceResponse | null;
}

export function Board({ board, gameState, lastRoll }: BoardProps) {
  // Group players by position
  const getPlayersAtPosition = (position: number) => {
    return gameState.players
      .map((player, index) => ({ player, index }))
      .filter(({ player }) => player.position === position);
  };

  // Get property owner
  const getPropertyOwner = (tileName: string) => {
    return gameState.allProperties.find(p => p.name === tileName) || null;
  };

  return (
    <div className="w-full flex items-center justify-center">
      {/* Board Grid */}
      <div className="grid grid-cols-11 grid-rows-11 gap-1 bg-black p-1.5 shadow-brutal w-full max-w-[85vw]">
        {board.tiles.map((tile) => {
          const { row, col } = getTileGridPosition(tile.position);
          return (
            <div
              key={tile.position}
              style={{
                gridRow: row + 1,
                gridColumn: col + 1,
              }}
            >
              <Tile
                tile={tile}
                playersOnTile={getPlayersAtPosition(tile.position)}
                owner={getPropertyOwner(tile.name)}
              />
            </div>
          );
        })}

        {/* Center area */}
        <div
          className="bg-white flex flex-col items-center justify-center p-3 gap-2"
          style={{
            gridRow: '2 / 11',
            gridColumn: '2 / 11',
          }}
        >
          <div className="text-3xl font-display font-black text-black uppercase tracking-tight">MONOPOLY</div>
          
          <TurnIndicator
            currentPlayerName={gameState.currentPlayerName}
            currentTurn={gameState.currentTurn}
          />

          {lastRoll && <DiceDisplay lastRoll={lastRoll} />}
        </div>
      </div>
    </div>
  );
}
