using ConsoleMonopolyApp.Enums;
using ConsoleMonopolyApp.Interfaces;

namespace ConsoleMonopolyApp.Models;

public class Player : IPlayer
{
    private static readonly char[] PlayerSymbols = { '1', '2', '3', '4' };
    private static int _playerCount = 0;

    public string Name { get; set; }
    public int RouteIndex { get; set; }
    public PlayerState State { get; set; }
    public IMoney Money { get; }
    public List<IAsset> Assets { get; }
    public ITile? CurrentTile { get; set; }
    public char Symbol { get; }
    public int JailTurns { get; set; }
    public bool HasGetOutOfJailCard { get; set; }

    public Player(string name, int initialMoney = 1500)
    {
        Name = name;
        Money = new Money(initialMoney);
        Assets = new List<IAsset>();
        RouteIndex = 0;
        State = PlayerState.Normal;
        CurrentTile = null;
        Symbol = PlayerSymbols[_playerCount % PlayerSymbols.Length];
        _playerCount++;
        JailTurns = 0;
        HasGetOutOfJailCard = false;
    }

    public void AddAsset(IAsset asset)
    {
        asset.Owner = this;
        Assets.Add(asset);
    }

    public void RemoveAsset(IAsset asset)
    {
        asset.Owner = null;
        Assets.Remove(asset);
    }

    public int GetTotalAssetValue()
    {
        int total = 0;
        foreach (var asset in Assets)
        {
            total += asset.Value;
            total += asset.AmountHouse * asset.HouseCost;
        }
        return total;
    }

    public int GetNetWorth()
    {
        return Money.Balance + GetTotalAssetValue();
    }

    public static void ResetPlayerCount()
    {
        _playerCount = 0;
    }

    public override string ToString()
    {
        return $"{Name} [{Symbol}] - {Money}";
    }
}
