using MonopolyApp.Enums;
using MonopolyApp.Interfaces;

namespace MonopolyApp.Models
{
    public class Asset : IAsset
    {
        public string Name {get; set;}
        public TypeAsset TypeAsset {get; set;}
        public AssetCondition AssetCondition {get; set;}
        public int Value {get; set;}
        public IPlayer? Owner {get; set;}
        public int AmountHouse {get; set;}

        public Asset(string name, TypeAsset typeAsset, AssetCondition assetCondition, int value, IPlayer owner, int amountHouse)
        {
            Name = name;
            TypeAsset = typeAsset;
            AssetCondition = assetCondition;
            Value = value;
            Owner = owner;
            AmountHouse = amountHouse;
        }
    }
}