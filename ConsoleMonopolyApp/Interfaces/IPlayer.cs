using ConsoleMonopolyApp.Enums;

namespace ConsoleMonopolyApp.Interfaces;

public interface IPlayer
{
    string Name { get; set; }
    int RouteIndex { get; set; }
    PlayerState State { get; set; }
    IMoney Money { get; }
    List<IAsset> Assets { get; }
    ITile? CurrentTile { get; set; }
    char Symbol { get; }
    int JailTurns { get; set; }
    bool HasGetOutOfJailCard { get; set; }
    
    void AddAsset(IAsset asset);
    void RemoveAsset(IAsset asset);
    int GetTotalAssetValue();
    int GetNetWorth();
}
