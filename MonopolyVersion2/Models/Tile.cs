using MonopolyApp.Enums;
using MonopolyApp.Interfaces;

namespace MonopolyApp.Models
{
    public class Tile : ITile
    {
        public string Name  {get; set;}
        public TilePos TilePos {get; set;}
        public int? PathIndex{get; set;}
        public char Display {get; set;}
        public EffectType EffectType {get; set;}
        public TypeAsset TypeAsset {get; set;}
        public AssetCondition AssetCondition {get; set;}
        public int Value {get; set;}
        public IPlayer? Owner {get; set;}  
        public int AmountHouse {get; set;}

        public Tile(string name, TilePos tilePos, char display, EffectType effectType, TypeAsset typeAsset, int value)
        {
            Name = name;
            TilePos = tilePos;
            Display = display;
            EffectType = effectType;
            TypeAsset = typeAsset;
            Value = value;
            AssetCondition = AssetCondition.Normal;
            AmountHouse = 0;
        }

    }
}