using MonopolyApp.Enums;
using MonopolyApp.Structs;

namespace MonopolyApp.Interfaces
{
    public interface ITile
    {
        string Name {get; set;}
        TilePos Pos {get; set;}
        int? PathIndex {get; set;}
        char Display {get; set;}
        TilesType TilesType {get; set;}
        EffectType EffectType {get; set;}
        
    }
}