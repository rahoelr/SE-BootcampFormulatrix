using ConsoleMonopolyApp.Enums;

namespace ConsoleMonopolyApp.Interfaces;

public interface IAsset
{
    string Name { get; set; }
    TypeAsset TypeAsset { get; set; }
    AssetsCondition AssetsCondition { get; set; }
    int Value { get; set; }
    IPlayer? Owner { get; set; }
    int AmountHouse { get; set; }
    int HouseCost { get; }
    int[] Rent { get; }
    int ColorGroup { get; }
    
    int CalculateRent(int diceRoll = 0, int sameColorCount = 0);
    int GetMortgageValue();
    int GetUnmortgageValue();
}
