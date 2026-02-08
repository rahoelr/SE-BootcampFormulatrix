import type { GameStateResponse, AvailableAction } from '../../types';

interface ActionPanelProps {
  gameState: GameStateResponse;
  onAction: (action: AvailableAction) => void;
  loading: boolean;
}

export function ActionPanel({ gameState, onAction, loading }: ActionPanelProps) {
  const currentPlayer = gameState.players.find(p => p.name === gameState.currentPlayerName);
  const isInJail = currentPlayer?.state === 'InJail';

  const getButtonConfig = (action: AvailableAction) => {
    const configs = {
      'roll-dice': { label: '🎲 Roll Dice', color: 'bg-brutal-yellow', size: 'large' },
      'buy-property': { label: '💰 Buy Property', color: 'bg-brutal-yellow', size: 'large' },
      'end-turn': { label: '✅ End Turn', color: 'bg-white', size: 'normal' },
      'pay-jail-fee': { label: '💵 Pay Jail Fee ($50)', color: 'bg-brutal-yellow', size: 'normal' },
      'use-jail-card': { label: '🎫 Use Get Out Card', color: 'bg-brutal-yellow', size: 'normal' },
      'try-roll-doubles': { label: '🎲 Try Roll Doubles', color: 'bg-brutal-yellow', size: 'normal' },
      'build-house': { label: '🏠 Build House', color: 'bg-white', size: 'small' },
      'sell-house': { label: '💸 Sell House', color: 'bg-white', size: 'small' },
      'mortgage-property': { label: '🔒 Mortgage', color: 'bg-white', size: 'small' },
      'unmortgage-property': { label: '🔓 Unmortgage', color: 'bg-white', size: 'small' },
      'trade': { label: '🔄 Trade', color: 'bg-white', size: 'small' },
    };
    return configs[action] || { label: action, color: 'bg-white', size: 'normal' };
  };

  const renderButton = (action: AvailableAction) => {
    const config = getButtonConfig(action);
    const sizeClass = 
      config.size === 'large' ? 'px-8 py-4 text-lg' :
      config.size === 'small' ? 'px-3 py-2 text-sm' :
      'px-6 py-3';

    return (
      <button
        key={action}
        onClick={() => onAction(action)}
        disabled={loading}
        className={`
          ${config.color} text-black font-display font-black uppercase
          ${sizeClass}
          border-4 border-black shadow-brutal
          hover:shadow-brutal-sm hover:translate-x-[2px] hover:translate-y-[2px]
          active:shadow-none active:translate-x-[4px] active:translate-y-[4px]
          disabled:opacity-50 disabled:cursor-not-allowed
          transition-all duration-100 tracking-wide
        `}
      >
        {config.label}
      </button>
    );
  };

  // Separate actions by category
  const primaryActions = gameState.availableActions.filter(a => 
    ['roll-dice', 'buy-property'].includes(a)
  );
  const jailActions = gameState.availableActions.filter(a => 
    ['pay-jail-fee', 'use-jail-card', 'try-roll-doubles'].includes(a)
  );
  const propertyActions = gameState.availableActions.filter(a => 
    ['build-house', 'sell-house', 'mortgage-property', 'unmortgage-property', 'trade'].includes(a)
  );
  const endTurnAction = gameState.availableActions.find(a => a === 'end-turn');

  return (
    <div className="bg-white border-4 border-black p-4 shadow-brutal space-y-4">
      <h3 className="text-xl font-display font-black text-black border-b-4 border-black pb-2 uppercase tracking-wide">
        Actions for {gameState.currentPlayerName}
      </h3>

      {/* Primary Actions */}
      {primaryActions.length > 0 && (
        <div className="flex flex-wrap gap-3">
          {primaryActions.map(renderButton)}
        </div>
      )}

      {/* Jail Actions */}
      {jailActions.length > 0 && isInJail && (
        <div className="bg-brutal-yellow border-4 border-black p-3 shadow-brutal-sm">
          <div className="text-sm font-display font-bold text-black mb-2 uppercase tracking-wide">🔒 Jail Options</div>
          <div className="flex flex-wrap gap-2">
            {jailActions.map(renderButton)}
          </div>
        </div>
      )}

      {/* Property Management */}
      {propertyActions.length > 0 && (
        <div className="bg-white border-4 border-black p-3 shadow-brutal-sm">
          <div className="text-sm font-display font-bold text-black mb-2 uppercase tracking-wide">🏠 Property Management</div>
          <div className="flex flex-wrap gap-2">
            {propertyActions.map(renderButton)}
          </div>
        </div>
      )}

      {/* End Turn (always at bottom) */}
      {endTurnAction && (
        <div className="pt-2 border-t-4 border-black">
          {renderButton(endTurnAction)}
        </div>
      )}
    </div>
  );
}
