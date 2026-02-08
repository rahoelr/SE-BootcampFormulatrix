import type { GameStateResponse, AvailableAction } from '../../types';
import { useState } from 'react';

interface ActionPanelProps {
  gameState: GameStateResponse;
  onAction: (action: AvailableAction) => void;
  onBuildHouse: (propertyName: string) => void;
  onSellHouse: (propertyName: string) => void;
  loading: boolean;
}

export function ActionPanel({ gameState, onAction, onBuildHouse, onSellHouse, loading }: ActionPanelProps) {
  const currentPlayer = gameState.players.find(p => p.name === gameState.currentPlayerName);
  const isInJail = currentPlayer?.state === 'InJail';
  const [selectedProperty, setSelectedProperty] = useState<string>('');

  const getButtonConfig = (action: AvailableAction) => {
    const configs = {
      'roll-dice': { label: '🎲 Roll Dice', color: 'bg-brutal-yellow', size: 'large' },
      'buy-property': { label: '💰 Buy Property', color: 'bg-brutal-yellow', size: 'large' },
      'end-turn': { label: '✅ End Turn', color: 'bg-white', size: 'normal' },
      'pay-jail-fee': { label: '💵 Pay Jail Fee ($50)', color: 'bg-brutal-yellow', size: 'normal' },
      'use-jail-card': { label: '🎫 Use Get Out Card', color: 'bg-brutal-yellow', size: 'normal' },
      'try-roll-doubles': { label: '🎲 Try Roll Doubles', color: 'bg-brutal-yellow', size: 'normal' },
      'build-house': { label: '🏠 Build House', color: 'bg-brutal-yellow', size: 'normal' },
      'sell-house': { label: '💸 Sell House', color: 'bg-white', size: 'normal' },
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
  // Hide trade and mortgage for now, only show build/sell
  const propertyActions = gameState.availableActions.filter(a => 
    ['build-house', 'sell-house'].includes(a)
  );
  const endTurnAction = gameState.availableActions.find(a => a === 'end-turn');

  // Get current player's properties for build/sell
  const buildableProperties = currentPlayer?.properties.filter(p => 
    !p.isMortgaged && p.houses < 5 && p.type === 'RealEstate'
  ) || [];

  const sellableProperties = currentPlayer?.properties.filter(p => p.houses > 0) || [];

  const selected = currentPlayer?.properties.find(p => p.name === selectedProperty);
  const canShowPropertyPanel = propertyActions.length > 0 && (buildableProperties.length > 0 || sellableProperties.length > 0);

  return (
    <div className="bg-white border-4 border-black p-4 shadow-brutal">
      <h3 className="text-xl font-display font-black text-black border-b-4 border-black pb-2 mb-4 uppercase tracking-wide">
        Actions for {gameState.currentPlayerName}
      </h3>

      <div className="flex flex-wrap items-center gap-3">
        {/* Primary Actions */}
        {primaryActions.length > 0 && (
          <>
            {primaryActions.map(renderButton)}
          </>
        )}

        {/* Jail Actions */}
        {jailActions.length > 0 && isInJail && (
          <>
            <div className="text-sm font-display font-bold text-black uppercase tracking-wide">🔒 Jail:</div>
            {jailActions.map(renderButton)}
          </>
        )}

        {/* End Turn (always at end) */}
        {endTurnAction && (
          <div className="ml-auto">
            {renderButton(endTurnAction)}
          </div>
        )}
      </div>

      {/* Property Management Panel - Build/Sell Houses */}
      {canShowPropertyPanel && (
        <div className="mt-4 pt-4 border-t-4 border-black">
          <h4 className="font-display font-black mb-3 uppercase tracking-wide text-lg">🏠 Build/Sell Houses</h4>

          <select
            value={selectedProperty}
            onChange={(e) => setSelectedProperty(e.target.value)}
            className="w-full border-4 border-black px-4 py-3 mb-3 font-body font-semibold shadow-brutal focus:shadow-brutal-sm focus:translate-x-[2px] focus:translate-y-[2px] transition-all duration-100"
            disabled={loading}
          >
            <option value="">-- Select Property --</option>
            {currentPlayer?.properties
              .filter(p => p.type === 'RealEstate')
              .map(property => (
                <option key={property.name} value={property.name}>
                  {property.name} ({property.houses === 5 ? 'Hotel' : `${property.houses} houses`})
                </option>
              ))}
          </select>

          {selected && (
            <div className="space-y-3">
              <div className="bg-brutal-yellow border-4 border-black p-3 shadow-brutal-sm">
                <div className="font-body font-semibold space-y-1 text-sm">
                  <div className="flex justify-between">
                    <span>Current:</span>
                    <span>{selected.houses === 5 ? '🏨 Hotel' : `${'🏠'.repeat(selected.houses || 0)} ${selected.houses}`}</span>
                  </div>
                  <div className="flex justify-between">
                    <span>Property Value:</span>
                    <span className="font-mono">${selected.value}</span>
                  </div>
                  <div className="flex justify-between font-bold">
                    <span>Build Cost:</span>
                    <span className="font-mono">${Math.floor(selected.value * 0.5)}</span>
                  </div>
                  <div className="flex justify-between font-bold">
                    <span>Sell Value:</span>
                    <span className="font-mono">${Math.floor(selected.value * 0.25)}</span>
                  </div>
                </div>
              </div>

              <div className="flex gap-2">
                {selected.houses < 5 && !selected.isMortgaged && (
                  <button
                    onClick={() => onBuildHouse(selected.name)}
                    disabled={loading}
                    className="flex-1 bg-brutal-yellow text-black px-4 py-3 font-display font-black uppercase border-4 border-black shadow-brutal hover:shadow-brutal-sm hover:translate-x-[2px] hover:translate-y-[2px] active:shadow-none active:translate-x-[4px] active:translate-y-[4px] transition-all duration-100 disabled:opacity-50"
                  >
                    🏠 Build House
                  </button>
                )}

                {selected.houses > 0 && (
                  <button
                    onClick={() => onSellHouse(selected.name)}
                    disabled={loading}
                    className="flex-1 bg-white text-black px-4 py-3 font-display font-black uppercase border-4 border-black shadow-brutal hover:shadow-brutal-sm hover:translate-x-[2px] hover:translate-y-[2px] active:shadow-none active:translate-x-[4px] active:translate-y-[4px] transition-all duration-100 disabled:opacity-50"
                  >
                    💸 Sell House
                  </button>
                )}
              </div>
            </div>
          )}
        </div>
      )}
    </div>
  );
}
