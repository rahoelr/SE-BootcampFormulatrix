import { useState } from 'react';
import type { PropertyResponse } from '../../types';

interface PropertyActionsProps {
  properties: PropertyResponse[];
  onBuild: (propertyName: string) => void;
  onSell: (propertyName: string) => void;
  loading: boolean;
}

export function PropertyActions({ properties, onBuild, onSell, loading }: PropertyActionsProps) {
  const [selectedProperty, setSelectedProperty] = useState<string>('');

  const buildableProperties = properties.filter(p => 
    !p.isMortgaged && p.houses < 5 && p.type === 'RealEstate'
  );

  const sellableProperties = properties.filter(p => p.houses > 0);

  if (buildableProperties.length === 0 && sellableProperties.length === 0) {
    return null;
  }

  const selected = properties.find(p => p.name === selectedProperty);

  return (
    <div className="bg-white border-4 border-black p-4 shadow-brutal">
      <h4 className="font-display font-black mb-3 uppercase tracking-wide">🏠 Build/Sell Houses</h4>

      <select
        value={selectedProperty}
        onChange={(e) => setSelectedProperty(e.target.value)}
        className="w-full border-3 border-black px-3 py-2 mb-3 font-body shadow-brutal-sm focus:shadow-brutal focus:translate-x-[2px] focus:translate-y-[2px] transition-all duration-100"
        disabled={loading}
      >
        <option value="">-- Select Property --</option>
        {properties
          .filter(p => p.type === 'RealEstate')
          .map(property => (
            <option key={property.name} value={property.name}>
              {property.name} ({property.houses === 5 ? 'Hotel' : `${property.houses} houses`})
            </option>
          ))}
      </select>

      {selected && (
        <div className="space-y-2">
          <div className="text-sm font-body font-semibold mb-2">
            <div>Current: {selected.houses === 5 ? '🏨 Hotel' : `${'🏠'.repeat(selected.houses || 0)} ${selected.houses} houses`}</div>
            <div>Property Value: ${selected.value}</div>
            <div>Build Cost: ${Math.floor(selected.value * 0.5)}</div>
            <div>Sell Value: ${Math.floor(selected.value * 0.25)}</div>
          </div>

          <div className="flex gap-2">
            {selected.houses < 5 && !selected.isMortgaged && (
              <button
                onClick={() => onBuild(selected.name)}
                disabled={loading}
                className="flex-1 bg-brutal-yellow text-black px-4 py-2 font-display font-black uppercase border-4 border-black shadow-brutal hover:shadow-brutal-sm hover:translate-x-[2px] hover:translate-y-[2px] active:shadow-none active:translate-x-[4px] active:translate-y-[4px] transition-all duration-100 disabled:opacity-50"
              >
                🏠 Build House
              </button>
            )}

            {selected.houses > 0 && (
              <button
                onClick={() => onSell(selected.name)}
                disabled={loading}
                className="flex-1 bg-white text-black px-4 py-2 font-display font-black uppercase border-4 border-black shadow-brutal hover:shadow-brutal-sm hover:translate-x-[2px] hover:translate-y-[2px] active:shadow-none active:translate-x-[4px] active:translate-y-[4px] transition-all duration-100 disabled:opacity-50"
              >
                💸 Sell House
              </button>
            )}
          </div>
        </div>
      )}
    </div>
  );
}
