import { useState } from 'react';
import type { PropertyResponse } from '../../types';

interface MortgagePanelProps {
  properties: PropertyResponse[];
  onMortgage: (propertyName: string) => void;
  onUnmortgage: (propertyName: string) => void;
  loading: boolean;
}

export function MortgagePanel({ properties, onMortgage, onUnmortgage, loading }: MortgagePanelProps) {
  const [selectedProperty, setSelectedProperty] = useState<string>('');

  if (properties.length === 0) {
    return null;
  }

  const selected = properties.find(p => p.name === selectedProperty);

  return (
    <div className="bg-white border-4 border-black p-4 shadow-brutal">
      <h4 className="font-display font-black mb-3 uppercase tracking-wide">🔒 Mortgage Properties</h4>

      <select
        value={selectedProperty}
        onChange={(e) => setSelectedProperty(e.target.value)}
        className="w-full border-3 border-black px-3 py-2 mb-3 font-body shadow-brutal-sm focus:shadow-brutal focus:translate-x-[2px] focus:translate-y-[2px] transition-all duration-100"
        disabled={loading}
      >
        <option value="">-- Select Property --</option>
        {properties.map(property => (
          <option key={property.name} value={property.name}>
            {property.name} {property.isMortgaged ? '(Mortgaged)' : ''}
          </option>
        ))}
      </select>

      {selected && (
        <div className="space-y-2">
          <div className="text-sm font-body font-semibold mb-2">
            <div>Status: {selected.isMortgaged ? '🔒 Mortgaged' : '✓ Active'}</div>
            <div>Property Value: ${selected.value}</div>
            <div>Mortgage Value: ${Math.floor(selected.value * 0.5)}</div>
            <div>Unmortgage Cost: ${Math.floor(selected.value * 0.55)}</div>
            {selected.houses > 0 && <div className="font-display font-black uppercase">⚠ Must sell all houses first</div>}
          </div>

          <div className="flex gap-2">
            {!selected.isMortgaged && selected.houses === 0 && (
              <button
                onClick={() => onMortgage(selected.name)}
                disabled={loading}
                className="flex-1 bg-black text-white px-4 py-2 font-display font-black uppercase border-4 border-black shadow-brutal hover:shadow-brutal-sm hover:translate-x-[2px] hover:translate-y-[2px] active:shadow-none active:translate-x-[4px] active:translate-y-[4px] transition-all duration-100 disabled:opacity-50"
              >
                🔒 Mortgage (Get ${Math.floor(selected.value * 0.5)})
              </button>
            )}

            {selected.isMortgaged && (
              <button
                onClick={() => onUnmortgage(selected.name)}
                disabled={loading}
                className="flex-1 bg-brutal-yellow text-black px-4 py-2 font-display font-black uppercase border-4 border-black shadow-brutal hover:shadow-brutal-sm hover:translate-x-[2px] hover:translate-y-[2px] active:shadow-none active:translate-x-[4px] active:translate-y-[4px] transition-all duration-100 disabled:opacity-50"
              >
                🔓 Unmortgage (Pay ${Math.floor(selected.value * 0.55)})
              </button>
            )}
          </div>
        </div>
      )}
    </div>
  );
}
