using MonopolyApp.Enums;

namespace MonopolyApp.Interfaces
{
    public interface ITile
    {
        string Name {get; set;}
        TilePos TilePos {get; set;}
        int? PathIndex {get; set;}
        char Display {get; set;}
        EffectType EffectType {get; set;}
        
    }
}