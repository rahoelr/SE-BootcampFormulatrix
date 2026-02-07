using MonopolyBackend.Interfaces;
using MonopolyBackend.Enums;

namespace MonopolyBackend.Interfaces
{
    public interface IAsset
    {
        public class Asset : IAsset
    {
        public string Name {get; set;}
        public TypeAsset TypeAsset {get; set;}
        public AssetCondition AssetCondition {get; set;}
        public int Value {get; set;}
        public IPlayer? Owner {get; set;}
        public int AmountHouse {get; set;} = 0;

        public Asset(string name, TypeAsset typeAsset, int value)
        {
            Name = name;
            TypeAsset = typeAsset;
            Value = value;
            AssetCondition = AssetCondition.Normal;
            Owner = null;
        }
    }

    }
}