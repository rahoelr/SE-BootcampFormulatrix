using MonopolyBackend.Enums;
using MonopolyBackend.Interfaces;
using MonopolyBackend.Structs;

namespace MonopolyBackend.Models
{
    public class Tile : ITile
    {
        public string Name { get; set; }
        public TilePos Pos { get; set; }
        public int? PathIndex { get; set; }
        public char Display { get; set; }
        public EffectType EffectType { get; set; }
        public TilesType TilesType { get; set; }
        public TypeAsset TypeAsset { get; set; }
        public AssetCondition AssetCondition { get; set; }
        public int Value { get; set; }
        public IPlayer? Owner { get; set; }
        public int AmountHouse { get; set; }

        public Tile(TilePos pos, string name, char display = ' ', TilesType type = TilesType.Special,
                    EffectType effectType = EffectType.Nothing, int? pathIndex = null)
        {
            Pos = pos;
            Name = name;
            Display = display;
            TilesType = type;
            EffectType = effectType;
            PathIndex = pathIndex;
        }

    }
}