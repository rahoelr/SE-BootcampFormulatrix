import { useState } from 'react';
import type { PlayerResponse, TradeRequest } from '../../types';

interface TradeModalProps {
  currentPlayer: PlayerResponse;
  allPlayers: PlayerResponse[];
  onTrade: (data: Omit<TradeRequest, 'playerName'>) => void;
  onClose: () => void;
  loading: boolean;
}

export function TradeModal({ currentPlayer, allPlayers, onTrade, onClose, loading }: TradeModalProps) {
  const [targetPlayer, setTargetPlayer] = useState<string>('');
  const [offeredProperties, setOfferedProperties] = useState<string[]>([]);
  const [offeredMoney, setOfferedMoney] = useState(0);
  const [requestedProperties, setRequestedProperties] = useState<string[]>([]);
  const [requestedMoney, setRequestedMoney] = useState(0);

  const otherPlayers = allPlayers.filter(p => 
    p.name !== currentPlayer.name && p.state !== 'Bankrupt'
  );

  const targetPlayerData = allPlayers.find(p => p.name === targetPlayer);

  const handleSubmit = () => {
    if (!targetPlayer) {
      alert('Please select a player to trade with');
      return;
    }

    onTrade({
      targetPlayerName: targetPlayer,
      offeredProperties,
      offeredMoney,
      requestedProperties,
      requestedMoney,
    });
  };

  const toggleOfferedProperty = (propertyName: string) => {
    setOfferedProperties(prev =>
      prev.includes(propertyName)
        ? prev.filter(p => p !== propertyName)
        : [...prev, propertyName]
    );
  };

  const toggleRequestedProperty = (propertyName: string) => {
    setRequestedProperties(prev =>
      prev.includes(propertyName)
        ? prev.filter(p => p !== propertyName)
        : [...prev, propertyName]
    );
  };

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
      <div className="bg-white border-4 border-black shadow-brutal-modal max-w-4xl w-full max-h-[90vh] overflow-y-auto">
        <div className="p-6">
          <div className="flex justify-between items-center mb-4">
            <h2 className="text-3xl font-display font-black uppercase tracking-tight">🔄 Trade Properties</h2>
            <button
              onClick={onClose}
              className="text-black hover:text-black text-3xl font-display font-black"
            >
              ×
            </button>
          </div>

          {/* Select Target Player */}
          <div className="mb-4">
            <label className="block font-display font-bold mb-2 uppercase tracking-wide">Trade With:</label>
            <select
              value={targetPlayer}
              onChange={(e) => setTargetPlayer(e.target.value)}
              className="w-full border-3 border-black px-3 py-2 font-body shadow-brutal-sm focus:shadow-brutal focus:translate-x-[2px] focus:translate-y-[2px] transition-all duration-100"
              disabled={loading}
            >
              <option value="">-- Select Player --</option>
              {otherPlayers.map(player => (
                <option key={player.name} value={player.name}>
                  {player.name} (${player.money})
                </option>
              ))}
            </select>
          </div>

          {targetPlayerData && (
            <div className="grid grid-cols-2 gap-4">
              {/* Your Offer */}
              <div className="border-4 border-black p-4 shadow-brutal">
                <h3 className="font-display font-black mb-3 text-black uppercase tracking-wide">Your Offer</h3>
                
                <div className="mb-4">
                  <label className="block text-sm font-display font-bold mb-2 uppercase">Money:</label>
                  <input
                    type="number"
                    value={offeredMoney}
                    onChange={(e) => setOfferedMoney(Number(e.target.value))}
                    min={0}
                    max={currentPlayer.money}
                    className="w-full border-3 border-black px-3 py-2 font-mono shadow-brutal-sm focus:shadow-brutal focus:translate-x-[2px] focus:translate-y-[2px] transition-all duration-100"
                  />
                  <div className="text-xs font-body font-semibold mt-1">
                    Available: ${currentPlayer.money}
                  </div>
                </div>

                <div>
                  <label className="block text-sm font-display font-bold mb-2 uppercase">Properties:</label>
                  <div className="space-y-1 max-h-40 overflow-y-auto">
                    {currentPlayer.properties.length > 0 ? (
                      currentPlayer.properties.map(property => (
                        <label key={property.name} className="flex items-center gap-2 cursor-pointer">
                          <input
                            type="checkbox"
                            checked={offeredProperties.includes(property.name)}
                            onChange={() => toggleOfferedProperty(property.name)}
                            className="w-4 h-4 border-2 border-black"
                          />
                          <span className="text-sm font-body font-semibold">
                            {property.name} (${property.value})
                          </span>
                        </label>
                      ))
                    ) : (
                      <div className="text-sm font-body">No properties</div>
                    )}
                  </div>
                </div>
              </div>

              {/* Your Request */}
              <div className="border-4 border-black p-4 shadow-brutal">
                <h3 className="font-display font-black mb-3 text-black uppercase tracking-wide">You Request</h3>
                
                <div className="mb-4">
                  <label className="block text-sm font-display font-bold mb-2 uppercase">Money:</label>
                  <input
                    type="number"
                    value={requestedMoney}
                    onChange={(e) => setRequestedMoney(Number(e.target.value))}
                    min={0}
                    max={targetPlayerData.money}
                    className="w-full border-3 border-black px-3 py-2 font-mono shadow-brutal-sm focus:shadow-brutal focus:translate-x-[2px] focus:translate-y-[2px] transition-all duration-100"
                  />
                  <div className="text-xs font-body font-semibold mt-1">
                    Available: ${targetPlayerData.money}
                  </div>
                </div>

                <div>
                  <label className="block text-sm font-display font-bold mb-2 uppercase">Properties:</label>
                  <div className="space-y-1 max-h-40 overflow-y-auto">
                    {targetPlayerData.properties.length > 0 ? (
                      targetPlayerData.properties.map(property => (
                        <label key={property.name} className="flex items-center gap-2 cursor-pointer">
                          <input
                            type="checkbox"
                            checked={requestedProperties.includes(property.name)}
                            onChange={() => toggleRequestedProperty(property.name)}
                            className="w-4 h-4 border-2 border-black"
                          />
                          <span className="text-sm font-body font-semibold">
                            {property.name} (${property.value})
                          </span>
                        </label>
                      ))
                    ) : (
                      <div className="text-sm font-body">No properties</div>
                    )}
                  </div>
                </div>
              </div>
            </div>
          )}

          {/* Action Buttons */}
          <div className="flex gap-3 mt-6">
            <button
              onClick={handleSubmit}
              disabled={loading || !targetPlayer}
              className="flex-1 bg-brutal-yellow text-black px-6 py-3 font-display font-black uppercase tracking-wide border-4 border-black shadow-brutal hover:shadow-brutal-sm hover:translate-x-[2px] hover:translate-y-[2px] active:shadow-none active:translate-x-[4px] active:translate-y-[4px] transition-all duration-100 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {loading ? 'Processing...' : 'Propose Trade'}
            </button>
            <button
              onClick={onClose}
              disabled={loading}
              className="px-6 py-3 bg-white text-black font-display font-black uppercase tracking-wide border-4 border-black shadow-brutal hover:shadow-brutal-sm hover:translate-x-[2px] hover:translate-y-[2px] active:shadow-none active:translate-x-[4px] active:translate-y-[4px] transition-all duration-100 disabled:opacity-50"
            >
              Cancel
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
