import { useState } from 'react';
import type { PropertyResponse } from '../../types';

interface PropertyActionModalProps {
  properties: PropertyResponse[];
  action: 'build' | 'sell';
  onConfirm: (propertyName: string) => void;
  onClose: () => void;
  loading: boolean;
}

export function PropertyActionModal({ properties, action, onConfirm, onClose, loading }: PropertyActionModalProps) {
  const [selectedProperty, setSelectedProperty] = useState<string>('');

  const availableProperties = properties.filter(p => {
    if (action === 'build') {
      return !p.isMortgaged && p.houses < 5 && p.type === 'RealEstate';
    } else {
      return p.houses > 0;
    }
  });

  const selected = properties.find(p => p.name === selectedProperty);

  const handleConfirm = () => {
    if (selectedProperty) {
      onConfirm(selectedProperty);
    }
  };

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
      <div className="bg-white border-4 border-black shadow-brutal-lg p-6 max-w-md w-full">
        <h2 className="text-2xl font-display font-black text-black mb-4 uppercase tracking-tight text-center">
          {action === 'build' ? '🏠 Build House' : '💸 Sell House'}
        </h2>
        
        {availableProperties.length === 0 ? (
          <div className="text-center mb-6">
            <p className="font-body text-black">
              {action === 'build' 
                ? 'No properties available for building.' 
                : 'No houses to sell.'}
            </p>
          </div>
        ) : (
          <>
            <select
              value={selectedProperty}
              onChange={(e) => setSelectedProperty(e.target.value)}
              className="w-full border-4 border-black px-4 py-3 mb-4 font-body font-semibold shadow-brutal focus:shadow-brutal-sm focus:translate-x-[2px] focus:translate-y-[2px] transition-all duration-100"
              disabled={loading}
            >
              <option value="">-- Select Property --</option>
              {availableProperties.map(property => (
                <option key={property.name} value={property.name}>
                  {property.name} ({property.houses === 5 ? 'Hotel' : `${property.houses} houses`})
                </option>
              ))}
            </select>

            {selected && (
              <div className="bg-brutal-yellow border-4 border-black p-4 mb-4 shadow-brutal-sm">
                <div className="font-body font-semibold space-y-1">
                  <div className="flex justify-between">
                    <span>Current:</span>
                    <span>{selected.houses === 5 ? '🏨 Hotel' : `${'🏠'.repeat(selected.houses || 0)} ${selected.houses}`}</span>
                  </div>
                  <div className="flex justify-between">
                    <span>Property Value:</span>
                    <span className="font-mono">${selected.value}</span>
                  </div>
                  {action === 'build' ? (
                    <div className="flex justify-between font-bold">
                      <span>Build Cost:</span>
                      <span className="font-mono">${Math.floor(selected.value * 0.5)}</span>
                    </div>
                  ) : (
                    <div className="flex justify-between font-bold">
                      <span>Sell Value:</span>
                      <span className="font-mono">${Math.floor(selected.value * 0.25)}</span>
                    </div>
                  )}
                </div>
              </div>
            )}
          </>
        )}

        <div className="flex gap-3">
          <button
            onClick={onClose}
            className="flex-1 bg-white text-black px-6 py-3 font-display font-bold uppercase tracking-wide border-4 border-black shadow-brutal hover:shadow-brutal-sm hover:translate-x-[2px] hover:translate-y-[2px] active:shadow-none active:translate-x-[4px] active:translate-y-[4px] transition-all duration-100"
          >
            Cancel
          </button>
          <button
            onClick={handleConfirm}
            disabled={!selectedProperty || loading}
            className="flex-1 bg-brutal-yellow text-black px-6 py-3 font-display font-black uppercase tracking-wide border-4 border-black shadow-brutal hover:shadow-brutal-sm hover:translate-x-[2px] hover:translate-y-[2px] active:shadow-none active:translate-x-[4px] active:translate-y-[4px] transition-all duration-100 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {action === 'build' ? 'Build' : 'Sell'}
          </button>
        </div>
      </div>
    </div>
  );
}
