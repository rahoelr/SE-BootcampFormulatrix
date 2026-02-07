using System.Data.SqlTypes;
using MonopolyBackend.Enums;

namespace MonopolyBackend.Interfaces
{
    public interface IPlayer
    {
        string Name { get; set; }
        int PathIndex { get; set; }
        IMoney Money { get; set; }
        List<IAsset> Assets { get; set; }
        ITile? CurrentTile { get; set; }
        PlayerState PlayerState { get; set; }
    }
}