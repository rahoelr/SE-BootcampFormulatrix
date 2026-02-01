using ConsoleMonopolyApp.Enums;
using ConsoleMonopolyApp.Structs;

namespace ConsoleMonopolyApp.Interfaces;

public interface ITile
{
    string Name { get; set; }
    TilePos Pos { get; set; }
    int? PathIndex { get; set; }
    char Display { get; set; }
    TilesType Type { get; set; }
    EffectType EffectType { get; set; }
    IAsset? Asset { get; set; }
}
