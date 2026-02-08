using MonopolyBackend.Enums;
using MonopolyBackend.Interfaces;

namespace MonopolyBackend.Models
{
    public class Asset : IAsset
    {
        public string Name {get; set;}
        public TypeAsset TypeAsset {get; set;}
        public AssetCondition AssetCondition {get; set;}
        public int Value {get; set;}
        public IPlayer? Owner {get; set;}
        public int AmountHouse {get; set;}

        public Asset(string name, TypeAsset typeAsset, int value)
        {
            Name = name;
            TypeAsset = typeAsset;
            Value = value;
            AssetCondition = AssetCondition.Normal;
            Owner = null;
            AmountHouse = 0;
        }
    }
}