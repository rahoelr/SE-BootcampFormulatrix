interface TurnIndicatorProps {
  currentPlayerName: string;
  currentTurn: number;
}

export function TurnIndicator({ currentPlayerName, currentTurn }: TurnIndicatorProps) {
  return (
    <div className="bg-brutal-yellow text-black border-4 border-black p-4 text-center shadow-brutal">
      <div className="text-sm font-display font-bold mb-1 uppercase tracking-wide">Turn {currentTurn + 1}</div>
      <div className="text-2xl font-display font-black uppercase">{currentPlayerName}</div>
      <div className="text-sm mt-1 font-body font-semibold uppercase">It's your turn!</div>
    </div>
  );
}
