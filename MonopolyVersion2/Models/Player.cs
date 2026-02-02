using MonopolyApp.Enums;
using MonopolyApp.Interfaces;

namespace MonopolyApp.Models
{
    public class Player : IPlayer
    {
        public string Name { get; set; }
        public int PathIndex { get; set; }
        public PlayerState PlayerState { get; set; }
        public IMoney Money { get; set; }
        public List<IAsset> Assets { get; set; }
        public ITile CurrentTile { get; set; }

        public Player(string name, IMoney money, ITile startingTile)
        {
            Name = name;
            PathIndex = 0;
            PlayerState = PlayerState.Normal;
            Money = money;
            Assets = new List<IAsset>();
            CurrentTile = startingTile;
        }
    }
}