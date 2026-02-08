import type { RollDiceResponse } from '../../types';

interface DiceDisplayProps {
  lastRoll: RollDiceResponse | null;
}

const diceUnicode = ['⚀', '⚁', '⚂', '⚃', '⚄', '⚅'];

export function DiceDisplay({ lastRoll }: DiceDisplayProps) {
  if (!lastRoll) return null;

  return (
    <div className="bg-brutal-yellow border-4 border-black p-4 shadow-brutal">
      <div className="text-center mb-2">
        <div className="text-sm font-display font-bold text-black uppercase tracking-wide">Last Roll</div>
      </div>
      
      <div className="flex items-center justify-center gap-4 mb-2">
        <div className="text-5xl">{diceUnicode[lastRoll.dice1 - 1]}</div>
        <div className="text-5xl">{diceUnicode[lastRoll.dice2 - 1]}</div>
      </div>

      <div className="text-center">
        <div className="text-3xl font-display font-black text-black">{lastRoll.total}</div>
        {lastRoll.isDouble && (
          <div className="text-sm font-display font-black text-black mt-1 uppercase tracking-wider">DOUBLES!</div>
        )}
      </div>

      <div className="text-sm text-black text-center mt-2 font-body">
        Landed on: <span className="font-bold uppercase">{lastRoll.landedTile}</span>
      </div>
    </div>
  );
}
