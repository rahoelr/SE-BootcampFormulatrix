using ConsoleMonopolyApp.Enums;
using ConsoleMonopolyApp.Interfaces;
using ConsoleMonopolyApp.Structs;

namespace ConsoleMonopolyApp.Models;

public class Tile : ITile
{
    public string Name { get; set; }
    public TilePos Pos { get; set; }
    public int? PathIndex { get; set; }
    public char Display { get; set; }
    public TilesType Type { get; set; }
    public EffectType EffectType { get; set; }
    public IAsset? Asset { get; set; }

    public Tile(TilePos pos, string name, char display = ' ', TilesType type = TilesType.SPECIAL, 
                EffectType effectType = EffectType.NOTHING, int? pathIndex = null)
    {
        Pos = pos;
        Name = name;
        Display = display;
        Type = type;
        EffectType = effectType;
        PathIndex = pathIndex;
        Asset = null;
    }

    public override string ToString()
    {
        return $"{Name} [{Display}]";
    }
}
