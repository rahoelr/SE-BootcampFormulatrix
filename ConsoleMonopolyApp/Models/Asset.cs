using ConsoleMonopolyApp.Enums;
using ConsoleMonopolyApp.Interfaces;

namespace ConsoleMonopolyApp.Models;

public class Asset : IAsset
{
    public string Name { get; set; }
    public TypeAsset TypeAsset { get; set; }
    public AssetsCondition AssetsCondition { get; set; }
    public int Value { get; set; }
    public IPlayer? Owner { get; set; }
    public int AmountHouse { get; set; }
    public int HouseCost { get; }
    public int[] Rent { get; }
    public int ColorGroup { get; }

    public Asset(string name, TypeAsset typeAsset, int value, int[] rent, int houseCost = 0, int colorGroup = 0)
    {
        Name = name;
        TypeAsset = typeAsset;
        Value = value;
        Rent = rent;
        HouseCost = houseCost;
        ColorGroup = colorGroup;
        AssetsCondition = AssetsCondition.NORMAL;
        Owner = null;
        AmountHouse = 0;
    }

    public int CalculateRent(int diceRoll = 0, int sameColorCount = 0)
    {
        if (AssetsCondition == AssetsCondition.MORTGAGED)
            return 0;

        switch (TypeAsset)
        {
            case TypeAsset.REAL_ESTATE:
                int rentIndex = Math.Min(AmountHouse, Rent.Length - 1);
                int baseRent = Rent[rentIndex];
                // Double rent if owner owns all properties in color group and no houses
                if (AmountHouse == 0 && sameColorCount >= GetColorGroupSize())
                    return baseRent * 2;
                return baseRent;

            case TypeAsset.RAILROAD:
                // Rent based on number of railroads owned
                int railroadRentIndex = Math.Min(sameColorCount - 1, Rent.Length - 1);
                return railroadRentIndex >= 0 ? Rent[railroadRentIndex] : Rent[0];

            case TypeAsset.PUBLIC_SERVICE:
                // Rent is multiplier * dice roll
                int multiplier = sameColorCount >= 2 ? 10 : 4;
                return diceRoll * multiplier;

            default:
                return 0;
        }
    }

    private int GetColorGroupSize()
    {
        // Color groups with 2 properties: Brown (1), Dark Blue (8)
        // All others have 3 properties
        return (ColorGroup == 1 || ColorGroup == 8) ? 2 : 3;
    }

    public int GetMortgageValue()
    {
        return Value / 2;
    }

    public int GetUnmortgageValue()
    {
        return (int)(GetMortgageValue() * 1.1);
    }

    public override string ToString()
    {
        return $"{Name} (${Value})";
    }
}
