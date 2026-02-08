import type { PropertyResponse } from '../../types';
import { useState } from 'react';

interface PropertyListProps {
  properties: PropertyResponse[];
}

export function PropertyList({ properties }: PropertyListProps) {
  const [expanded, setExpanded] = useState(false);

  if (properties.length === 0) {
    return (
      <div className="text-sm text-black font-body italic">
        No properties owned
      </div>
    );
  }

  // Group by color
  const groupedProperties: Record<string, PropertyResponse[]> = {};
  properties.forEach(prop => {
    const color = prop.type;
    if (!groupedProperties[color]) {
      groupedProperties[color] = [];
    }
    groupedProperties[color].push(prop);
  });

  return (
    <div className="mt-2">
      <button
        onClick={() => setExpanded(!expanded)}
        className="text-sm font-display font-bold text-black hover:text-black flex items-center gap-1 uppercase tracking-wide"
      >
        {expanded ? '▼' : '▶'} Properties ({properties.length})
      </button>

      {expanded && (
        <div className="mt-2 space-y-2 max-h-60 overflow-y-auto">
          {properties.map((property) => (
            <div
              key={property.name}
              className="bg-white px-2 py-1 text-xs border-3 border-black shadow-brutal-sm"
            >
              <div className="flex items-center gap-2">
                <div className="flex-1">
                  <div className="font-display font-bold uppercase">{property.name}</div>
                  <div className="font-mono font-semibold">
                    ${property.value}
                    {property.houses > 0 && (
                      <span className="ml-2">
                        {property.houses === 5 ? '🏨' : `${'🏠'.repeat(property.houses)}`}
                      </span>
                    )}
                    {property.isMortgaged && <span className="ml-2 font-display font-bold uppercase">🔒 Mortgaged</span>}
                  </div>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
