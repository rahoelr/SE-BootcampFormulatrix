using System.Numerics;
using MonopolyApp.Enums;

namespace MonopolyApp.Interfaces
{
    public interface IAsset
    {
        string Name { get; set; }
        TypeAsset TypeAsset { get; set; }
        AssetCondition AssetCondition {get; set;}
        int Value { get; set; }
        IPlayer? Owner {get; set;}
        int AmountHouse {get; set;}


    }
}