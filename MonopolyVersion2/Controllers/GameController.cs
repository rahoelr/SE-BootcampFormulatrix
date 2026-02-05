using MonopolyApp.Enums;
using MonopolyApp.Interfaces;
using MonopolyApp.Models;

namespace MonopolyApp.Controllers
{
    public class GameController
    {
        public IBoard Board { get; }
        public List<IPlayer> Players { get; set; }
        public List<IDice> Dices { get; set; }
        public Dictionary<IPlayer, List<IAsset>> PlayerAssets { get; set; }
        public Dictionary<IPlayer, List<IMoney>> PlayerMoney { get; set; }
        public Dictionary<ITile, IAsset?> TileAssets { get; }
        public IDecks? ChanceDeck { get; }
        public int CurrentTurn { get; private set; }
        public IPlayer CurrentPlayer => Players[CurrentTurn % Players.Count];
        public bool IsGameOver { get; set; }
        public IPlayer? Winner { get; set; }

        private const int GO_SALARY = 200;
        private const int JAIL_POSITION = 10;
        private const int JAIL_FEE = 50;
        private const int TAX_AMOUNT = 200;
        private const int LUXURY_TAX = 100;

        public event Action<string>? OnMessage;
        public event Action<IPlayer, int, int>? OnDiceRolled;
        public event Action<IPlayer, ITile>? OnPlayerMoved;
        public event Action<IPlayer, IAsset>? OnPropertyBought;
        public event Action<IPlayer, int>? OnRentPaid;
        public event Action<ICard>? OnCardDrawn;
        public event Action<IPlayer>? OnPlayerBankrupt;
        public event Action<IPlayer>? OnPlayerWins;

        public GameController(IBoard board, List<IPlayer> players, List<IDice> dices, IDecks chanceDeck)
        {
            if (players.Count < 2 || players.Count > 4)
            {
                throw new ArgumentException("Game membutuhkan 2-4 pemain");
            }

            Board = board;
            Players = players;
            Dices = dices;
            ChanceDeck = chanceDeck;
            CurrentTurn = 0;
            IsGameOver = false;
            Winner = null;

            PlayerAssets = new Dictionary<IPlayer, List<IAsset>>();
            TileAssets = new Dictionary<ITile, IAsset?>();
            PlayerMoney = new Dictionary<IPlayer, List<IMoney>>();

            foreach (var player in players)
            {
                PlayerAssets[player] = new List<IAsset>();
                PlayerMoney[player] = new List<IMoney>();
                player.PathIndex = 0;
                player.CurrentTile = Board.Path[0];
            }

            foreach (var tile in Board.Path)
            {
                TileAssets[tile] = null;
            }
        }

        public void StartGame()
        {
            OnMessage?.Invoke("Game dimulai! Gas bro!");
            OnMessage?.Invoke($"Pemain : {string.Join(", ", Players.Select(p => p.Name))}");
            OnMessage?.Invoke($"Giliran : {CurrentPlayer.Name}");
        }

    }
}