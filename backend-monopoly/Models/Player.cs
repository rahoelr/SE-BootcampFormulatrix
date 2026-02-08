using MonopolyBackend.Enums;
using MonopolyBackend.Interfaces;

namespace MonopolyBackend.Models
{
    public class Player : IPlayer
    {
        public string Name { get; set; }
        public int PathIndex { get; set; }
        public PlayerState PlayerState { get; set; }
        public IMoney Money { get; set; }
        public List<IAsset> Assets { get; set; }
        public ITile? CurrentTile { get; set; }

        public Player(string name, IMoney money)
        {
            Name = name;
            PathIndex = 0;
            Money = money;
            Assets = new List<IAsset>();
            PlayerState = PlayerState.Normal;
            CurrentTile = null!;
        }
    }
}