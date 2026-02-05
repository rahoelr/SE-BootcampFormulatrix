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
        public IDecks CommunityChestDeck { get; }
        public IDecks ChanceDeck { get; }
        public int CurrentTurn { get; private set; }
        public IPlayer CurrentPlayer => Players[CurrentTurn % Players.Count];
        public bool IsGameOver { get; set; }
        public IPlayer? Winner { get; set; }
        private Dictionary<IPlayer, int> _playerJailTurns { get; set; }
        private Dictionary<IPlayer, int> _playerGetOutOfJailCards { get; set; }

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

        public GameController(IBoard board, List<IPlayer> players, List<IDice> dices, IDecks communityChestDeck, IDecks chanceDeck)
        {
            if (players.Count < 2 || players.Count > 4)
            {
                throw new ArgumentException("Game membutuhkan 2-4 pemain");
            }

            Board = board;
            Players = players;
            Dices = dices;
            ChanceDeck = chanceDeck;
            CommunityChestDeck = communityChestDeck;
            CurrentTurn = 0;
            IsGameOver = false;
            Winner = null;
            _playerGetOutOfJailCards = new Dictionary<IPlayer, int>();
            foreach (var player in players)
            {
                _playerGetOutOfJailCards[player] = 0;
            }
            _playerJailTurns = new Dictionary<IPlayer, int>();

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

        public void NextTurn()
        {
            // Skip bankrupt players
            do
            {
                CurrentTurn++;
            } while (CurrentPlayer.PlayerState == PlayerState.Bankrupt && GetActivePlayers().Count > 1);

            var activePlayers = GetActivePlayers();
            if (activePlayers.Count == 1)
            {
                IsGameOver = true;
                Winner = activePlayers[0];
                OnPlayerWins?.Invoke(Winner);
                return;
            }

            OnMessage?.Invoke($"Giliran : {CurrentPlayer.Name}");

        }

        public (int dice1, int dice2) RollDices()
        {
            Random rand = new Random();
            int dice1 = rand.Next(1, 7);
            int dice2 = rand.Next(1, 7);
            OnDiceRolled?.Invoke(CurrentPlayer, dice1, dice2);
            int totalMove = dice1 + dice2;
            OnMessage?.Invoke($"{CurrentPlayer.Name} melempar dadu dan mendapatkan {dice1} dan {dice2} dengan total {totalMove}");
            OnDiceRolled?.Invoke(CurrentPlayer, dice1, dice2);
            return (dice1, dice2);
        }

        public List<IPlayer> GetActivePlayers()
        {
            return Players.Where(p => p.PlayerState != PlayerState.Bankrupt).ToList();
        }

        public bool HandleJailTurn()
        {
            if (CurrentPlayer.PlayerState != PlayerState.InJail)
                return false;

            if (!_playerJailTurns.ContainsKey(CurrentPlayer))
                _playerJailTurns[CurrentPlayer] = 0;

            _playerJailTurns[CurrentPlayer]++;
            int jailTurns = _playerJailTurns[CurrentPlayer];

            OnMessage?.Invoke($"{CurrentPlayer.Name} di penjara, giliran ke-{jailTurns}");

            // After 3 turns, must pay
            if (jailTurns >= 3)
            {
                OnMessage?.Invoke($"{CurrentPlayer.Name} sudah 3 giliran di penjara, harus membayar ${JAIL_FEE}");
                return PayJailFee();
            }

            // Options: Pay $50, Use Get Out of Jail card, or try to roll doubles
            return true;
        }

        public bool PayJailFee()
        {
            if (CurrentPlayer.PlayerState != PlayerState.InJail)
                return false;

            if (!SubtractMoney(CurrentPlayer, JAIL_FEE))
                return false;

            CurrentPlayer.PlayerState = PlayerState.Normal;
            _playerJailTurns[CurrentPlayer] = 0;
            OnMessage?.Invoke($"{CurrentPlayer.Name} keluar dari penjara.");
            return true;
        }

        public bool UseGetOutOfJailCard()
        {
            if (CurrentPlayer.PlayerState != PlayerState.InJail)
                return false;

            if (_playerGetOutOfJailCards[CurrentPlayer] <= 0)
            {
                OnMessage?.Invoke($"{CurrentPlayer.Name} tidak memiliki kartu bebas penjara.");
                return false;
            }

            _playerGetOutOfJailCards[CurrentPlayer]--;
            CurrentPlayer.PlayerState = PlayerState.Normal;
            _playerJailTurns[CurrentPlayer] = 0;
            OnMessage?.Invoke($"{CurrentPlayer.Name} menggunakan kartu bebas penjara!");
            return true;
        }

        public int GetJailTurns(IPlayer player)
        {
            return _playerJailTurns.ContainsKey(player) ? _playerJailTurns[player] : 0;
        }

        public bool HasGetOutOfJailCard(IPlayer player)
        {
            return _playerGetOutOfJailCards.ContainsKey(player) &&
                   _playerGetOutOfJailCards[player] > 0;
        }

        public bool TryRollDoublesInJail()
        {
            if (CurrentPlayer.PlayerState != PlayerState.InJail)
                return false;

            var (dice1, dice2) = RollDices();

            if (dice1 == dice2)
            {
                // Berhasil roll ganda - keluar dari penjara
                CurrentPlayer.PlayerState = PlayerState.Normal;
                _playerJailTurns[CurrentPlayer] = 0;
                OnMessage?.Invoke($"{CurrentPlayer.Name} melempar ganda dan keluar dari penjara!");

                // Pindahkan pemain
                MovePlayer(dice1 + dice2);
                OnLand();
                return true;
            }
            else
            {
                OnMessage?.Invoke($"{CurrentPlayer.Name} tidak mendapat ganda. Tetap di penjara.");
                return false;
            }
        }

        public bool AddMoney(IPlayer player, int amount)
        {
            if (amount <= 0)
                return false;

            var money = new Money(amount);
            PlayerMoney[player].Add(money);
            OnMessage?.Invoke($"{player.Name} menerima ${amount}");
            return true;
        }

        public bool SubtractMoney(IPlayer player, int amount)
        {
            if (amount <= 0)
                return false;

            // Check if player has enough money
            int currentMoney = PlayerMoney[player].Sum(m => m.Balance);
            if (currentMoney < amount)
            {
                OnMessage?.Invoke($"{player.Name} tidak punya cukup uang. Dibutuhkan ${amount}, punya ${currentMoney}");
                return false;
            }

            // Deduct money
            int remaining = amount;
            var moneyList = PlayerMoney[player].OrderByDescending(m => m.Balance).ToList();

            foreach (var money in moneyList)
            {
                if (remaining <= 0) break;

                if (money.Balance <= remaining)
                {
                    remaining -= money.Balance;
                    PlayerMoney[player].Remove(money);
                }
            }

            OnMessage?.Invoke($"{player.Name} membayar ${amount}");
            return true;
        }

        public int GetPlayerMoney(IPlayer player)
        {
            return PlayerMoney[player].Sum(m => m.Balance);
        }

        public ITile MovePlayer(int steps)
        {
            int oldPosition = CurrentPlayer.PathIndex;
            int newPosition = (oldPosition + steps) % Board.Path.Count;

            // Check if passed GO
            if (newPosition < oldPosition && steps > 0)
            {
                AddMoney(CurrentPlayer, GO_SALARY);
                OnMessage?.Invoke($"{CurrentPlayer.Name} melewati MULAI dan menerima ${GO_SALARY}!");
            }

            CurrentPlayer.PathIndex = newPosition;
            CurrentPlayer.CurrentTile = Board.Path[newPosition];

            OnPlayerMoved?.Invoke(CurrentPlayer, CurrentPlayer.CurrentTile);
            OnMessage?.Invoke($"{CurrentPlayer.Name} mendarat di {CurrentPlayer.CurrentTile.Name}");

            return CurrentPlayer.CurrentTile;
        }

        public void MovePlayerToPosition(int position)
        {
            int oldPosition = CurrentPlayer.PathIndex;

            // Check if passed GO
            if (position < oldPosition)
            {
                AddMoney(CurrentPlayer, GO_SALARY);
                OnMessage?.Invoke($"{CurrentPlayer.Name} passed GO and collected ${GO_SALARY}!");
            }

            CurrentPlayer.PathIndex = position;
            CurrentPlayer.CurrentTile = Board.Path[position];
            OnPlayerMoved?.Invoke(CurrentPlayer, CurrentPlayer.CurrentTile);
        }

        public void SendToJail()
        {
            CurrentPlayer.PathIndex = JAIL_POSITION;
            CurrentPlayer.CurrentTile = Board.Path[JAIL_POSITION];
            CurrentPlayer.PlayerState = PlayerState.InJail;
            _playerJailTurns[CurrentPlayer] = 0;
            OnMessage?.Invoke($"{CurrentPlayer.Name} was sent to Jail!");
        }

        public void OnLand()
        {
            var tile = CurrentPlayer.CurrentTile;
            if (tile == null) return;

            switch (tile.EffectType)
            {
                case EffectType.Go:
                    // Already handled in MovePlayer if passing GO
                    OnMessage?.Invoke($"{CurrentPlayer.Name} berada di MULAI.");
                    break;

                case EffectType.CommunityChest:
                    GetAndApplyDeck(CommunityChestDeck);
                    break;

                case EffectType.Chance:
                    GetAndApplyDeck(ChanceDeck);
                    break;

                case EffectType.Tax:
                    int taxAmount = tile.Name.Contains("Mewah") ? LUXURY_TAX : TAX_AMOUNT;
                    if (!SubtractMoney(CurrentPlayer, taxAmount))
                    {
                        CheckIsBankrupt(CurrentPlayer);
                    }
                    break;

                case EffectType.GoToJail:
                    SendToJail();
                    break;

                case EffectType.FreeParking:
                    OnMessage?.Invoke($"{CurrentPlayer.Name} parkir gratis.");
                    break;

                case EffectType.Nothing:
                    // Property tiles - check if tile has owner/is purchasable
                    var asset = TileAssets.ContainsKey(tile) ? TileAssets[tile] : null;
                    if (asset != null || tile.TilesType == TilesType.Property || tile.TilesType == TilesType.Railroad || tile.TilesType == TilesType.Utility)
                    {
                        HandlePropertyTile(tile);
                    }
                    break;
            }
        }

        private void HandlePropertyTile(ITile tile)
        {
            // Get asset from TileAssets dictionary
            if (!TileAssets.ContainsKey(tile) || TileAssets[tile] == null)
            {
                OnMessage?.Invoke($"{tile.Name} tidak memiliki asset.");
                return;
            }

            var asset = TileAssets[tile]!;

            if (asset.Owner == null)
            {
                // Property available for purchase
                OnMessage?.Invoke($"{tile.Name} tersedia untuk dibeli seharga ${asset.Value}");
            }
            else if (asset.Owner != CurrentPlayer)
            {
                // Pay rent
                if (asset.AssetCondition != AssetCondition.Mortgage)
                {
                    int rent = CalculateRent(asset);

                    if (SubtractMoney(CurrentPlayer, rent))
                    {
                        AddMoney(asset.Owner, rent);
                        OnRentPaid?.Invoke(CurrentPlayer, rent);
                        OnMessage?.Invoke($"{CurrentPlayer.Name} membayar sewa ${rent} kepada {asset.Owner.Name}");
                    }
                    else
                    {
                        OnMessage?.Invoke($"{CurrentPlayer.Name} tidak mampu membayar sewa ${rent}!");
                        CheckIsBankrupt(CurrentPlayer);
                    }
                }
                else
                {
                    OnMessage?.Invoke($"{tile.Name} sedang di-mortgage. Tidak ada sewa.");
                }
            }
            else
            {
                OnMessage?.Invoke($"{CurrentPlayer.Name} memiliki properti ini.");
            }
        }

        private int CalculateRent(IAsset asset)
        {
            int sameTypeCount = CountSameTypeAssets(asset.Owner!, asset);

            // Utility (Perusahaan Listrik/Air) - Fixed price
            if (asset.TypeAsset == TypeAsset.PublicService)
            {
                // Jika punya 1 utility: $25
                // Jika punya 2 utility: $50
                return sameTypeCount == 2 ? 50 : 25;
            }

            // Railroad (Stasiun) - Fixed price berdasarkan jumlah stasiun
            if (asset.TypeAsset == TypeAsset.Railroad)
            {
                // 1 stasiun: $25
                // 2 stasiun: $50
                // 3 stasiun: $100
                // 4 stasiun: $200
                return sameTypeCount switch
                {
                    1 => 25,
                    2 => 50,
                    3 => 100,
                    4 => 200,
                    _ => 25
                };
            }

            // Property (RealEstate) - Calculate rent based on houses
            // Base rent: 10% dari nilai property
            int baseRent = asset.Value / 10;

            // Jika ada rumah, rent meningkat
            if (asset.AmountHouse > 0)
            {
                // 1 rumah: base rent × 5
                // 2 rumah: base rent × 15
                // 3 rumah: base rent × 45
                // 4 rumah: base rent × 80
                // Hotel (5): base rent × 100
                return asset.AmountHouse switch
                {
                    1 => baseRent * 5,
                    2 => baseRent * 15,
                    3 => baseRent * 45,
                    4 => baseRent * 80,
                    5 => baseRent * 100,  // Hotel
                    _ => baseRent
                };
            }

            // Return base rent
            return baseRent;
        }

        private int CountSameTypeAssets(IPlayer owner, IAsset asset)
        {
            // Langsung count berdasarkan TypeAsset
            return owner.Assets.Count(a => a.TypeAsset == asset.TypeAsset);
        }

        public ICard DrawCardFromDeck(IDecks deck)
        {
            if (deck == null)
                throw new ArgumentNullException(nameof(deck));

            // Implementasi draw card langsung di sini
            // Asumsi: deck memiliki property Cards yang bisa diakses
            var cards = deck.Cards;
            if (cards == null || cards.Count == 0)
                throw new InvalidOperationException("Deck kosong");

            // Ambil kartu pertama dan pindahkan ke belakang (shuffle)
            var card = cards[0];
            cards.RemoveAt(0);
            cards.Add(card);

            OnCardDrawn?.Invoke(card);
            OnMessage?.Invoke($"Kartu: {card.Name} - {card.Description}");

            return card;
        }

        public void ApplyCardEffect(ICard card)
        {
            switch (card.CardEffect)
            {
                case CardEffect.ReceiveMoney:
                    AddMoney(CurrentPlayer, card.Value);
                    break;

                case CardEffect.PayMoney:
                    if (!SubtractMoney(CurrentPlayer, card.Value))
                    {
                        CheckIsBankrupt(CurrentPlayer);
                    }
                    break;

                case CardEffect.GoToJail:
                    SendToJail();
                    break;

                case CardEffect.GetOutJail:
                    _playerGetOutOfJailCards[CurrentPlayer]++;
                    OnMessage?.Invoke($"{CurrentPlayer.Name} menerima kartu Bebas Penjara!");
                    break;

                case CardEffect.Move:
                    if (card.Value < 0)
                    {
                        MovePlayer(card.Value);
                    }
                    else
                    {
                        MovePlayerToPosition(card.Value);
                    }
                    OnLand();
                    break;
            }
        }

        public bool PlayerBuyAsset(IAsset asset)
        {
            if (asset.Owner != null)
            {
                OnMessage?.Invoke("Properti ini sudah dimiliki.");
                return false;
            }

            if (!SubtractMoney(CurrentPlayer, asset.Value))
            {
                OnMessage?.Invoke($"{CurrentPlayer.Name} tidak punya cukup uang untuk membeli {asset.Name}.");
                return false;
            }

            // Set owner dan tambahkan ke assets
            asset.Owner = CurrentPlayer;
            CurrentPlayer.Assets.Add(asset);
            PlayerAssets[CurrentPlayer].Add(asset);

            OnPropertyBought?.Invoke(CurrentPlayer, asset);
            OnMessage?.Invoke($"{CurrentPlayer.Name} membeli {asset.Name} seharga ${asset.Value}!");
            return true;
        }

        public bool PlayerMortgageAsset(IPlayer player, IAsset asset)
        {
            if (asset.Owner != player)
            {
                OnMessage?.Invoke("Player doesn't own this property.");
                return false;
            }

            if (asset.AssetCondition == AssetCondition.Mortgage)
            {
                OnMessage?.Invoke("Property is already mortgaged.");
                return false;
            }

            if (asset.AmountHouse > 0)
            {
                OnMessage?.Invoke("Must sell all houses before mortgaging.");
                return false;
            }

            asset.AssetCondition = AssetCondition.Mortgage;
            int mortgageValue = GetMortgageValue(asset);
            AddMoney(player, mortgageValue);
            OnMessage?.Invoke($"{player.Name} mortgaged {asset.Name} for ${mortgageValue}.");
            return true;
        }

        public bool PlayerUnmortgageAsset(IPlayer player, IAsset asset)
        {
            if (asset.Owner != player)
            {
                OnMessage?.Invoke("Player doesn't own this property.");
                return false;
            }

            if (asset.AssetCondition != AssetCondition.Mortgage)
            {
                OnMessage?.Invoke("Property is not mortgaged.");
                return false;
            }

            int unmortgageValue = GetUnmortgageCost(asset);
            if (!SubtractMoney(player, unmortgageValue))
            {
                OnMessage?.Invoke($"Not enough money to unmortgage. Need ${unmortgageValue}.");
                return false;
            }

            asset.AssetCondition = AssetCondition.Normal;
            OnMessage?.Invoke($"{player.Name} unmortgaged {asset.Name} for ${unmortgageValue}.");
            return true;
        }

        public bool PlayerAddHouse(IAsset asset)
        {
            if (asset.Owner != CurrentPlayer)
            {
                OnMessage?.Invoke("Anda tidak memiliki properti ini.");
                return false;
            }

            if (asset.TypeAsset != TypeAsset.RealEstate)
            {
                OnMessage?.Invoke("Hanya bisa membangun rumah di properti RealEstate.");
                return false;
            }

            if (asset.AmountHouse >= 5)
            {
                OnMessage?.Invoke("Maksimum rumah (hotel) sudah dibangun.");
                return false;
            }

            // Hitung biaya rumah (50% dari nilai properti)
            int houseCost = asset.Value / 2;

            if (!SubtractMoney(CurrentPlayer, houseCost))
            {
                OnMessage?.Invoke($"Uang tidak cukup. Rumah berharga ${houseCost}.");
                return false;
            }

            asset.AmountHouse++;
            string buildingType = asset.AmountHouse == 5 ? "hotel" : "rumah";
            OnMessage?.Invoke($"{CurrentPlayer.Name} membangun {buildingType} di {asset.Name}.");
            return true;
        }

        public bool PlayerSellHouse(IAsset asset)
        {
            if (asset.Owner != CurrentPlayer)
            {
                OnMessage?.Invoke("Anda tidak memiliki properti ini.");
                return false;
            }

            if (asset.TypeAsset != TypeAsset.RealEstate)
            {
                OnMessage?.Invoke("Properti ini tidak memiliki rumah.");
                return false;
            }

            if (asset.AmountHouse <= 0)
            {
                OnMessage?.Invoke("Tidak ada rumah untuk dijual.");
                return false;
            }

            // Jual rumah dengan harga 50% dari harga beli
            int sellPrice = asset.Value / 4;
            asset.AmountHouse--;
            AddMoney(CurrentPlayer, sellPrice);

            string buildingType = asset.AmountHouse == 4 ? "hotel" : "rumah";
            OnMessage?.Invoke($"{CurrentPlayer.Name} menjual {buildingType} di {asset.Name} seharga ${sellPrice}.");
            return true;
        }

        public bool PlayerProposeTrade(IPlayer player1, IPlayer player2,
                                List<IAsset> offer1, int money1,
                                List<IAsset> offer2, int money2)
        {
            // Validate ownership
            foreach (var asset in offer1)
            {
                if (asset.Owner != player1)
                {
                    OnMessage?.Invoke($"{player1.Name} tidak memiliki {asset.Name}.");
                    return false;
                }
            }

            foreach (var asset in offer2)
            {
                if (asset.Owner != player2)
                {
                    OnMessage?.Invoke($"{player2.Name} tidak memiliki {asset.Name}.");
                    return false;
                }
            }

            // Check money using PlayerMoney dictionary
            int player1Money = GetPlayerMoney(player1);
            int player2Money = GetPlayerMoney(player2);

            if (player1Money < money1)
            {
                OnMessage?.Invoke($"{player1.Name} tidak punya ${money1}.");
                return false;
            }

            if (player2Money < money2)
            {
                OnMessage?.Invoke($"{player2.Name} tidak punya ${money2}.");
                return false;
            }

            // Execute trade - Transfer assets
            foreach (var asset in offer1)
            {
                asset.Owner = player2;
                player1.Assets.Remove(asset);
                PlayerAssets[player1].Remove(asset);
                player2.Assets.Add(asset);
                PlayerAssets[player2].Add(asset);
            }

            foreach (var asset in offer2)
            {
                asset.Owner = player1;
                player2.Assets.Remove(asset);
                PlayerAssets[player2].Remove(asset);
                player1.Assets.Add(asset);
                PlayerAssets[player1].Add(asset);
            }

            // Transfer money using existing methods
            if (money1 > 0)
            {
                SubtractMoney(player1, money1);
                AddMoney(player2, money1);
            }

            if (money2 > 0)
            {
                SubtractMoney(player2, money2);
                AddMoney(player1, money2);
            }

            OnMessage?.Invoke($"Perdagangan selesai antara {player1.Name} dan {player2.Name}!");
            return true;
        }

        public void GetAndApplyDeck(IDecks deck)
        {
            var card = DrawCardFromDeck(deck);
            ApplyCardEffect(card);
        }

        public bool CheckIsBankrupt(IPlayer player)
        {
            int totalValue = CalculatePlayerTotalAssetsValue(player);

            if (player.Money.Balance + totalValue < 0)
            {
                player.PlayerState = PlayerState.Bankrupt;
                OnPlayerBankrupt?.Invoke(player);
                OnMessage?.Invoke($"{player.Name} is BANKRUPT!");

                // Return assets to bank
                foreach (var asset in player.Assets.ToList())
                {
                    asset.Owner = null;
                    asset.AmountHouse = 0;
                    asset.AssetCondition = AssetCondition.Normal;
                }
                player.Assets.Clear();
                PlayerAssets[player].Clear();

                // Check for winner
                var activePlayers = GetActivePlayers();
                if (activePlayers.Count == 1)
                {
                    IsGameOver = true;
                    Winner = activePlayers[0];
                    OnPlayerWins?.Invoke(Winner);
                }

                return true;
            }

            return false;
        }

        public int CalculatePlayerTotalAssetsValue(IPlayer player)
        {
            int total = 0;
            foreach (var asset in player.Assets)
            {
                if (asset.AssetCondition == AssetCondition.Mortgage)
                {
                    total += GetMortgageValue(asset);
                }
                else
                {
                    total += asset.Value;
                }
                total += asset.AmountHouse * (asset.Value / 2);
            }
            return total;
        }

        public int GetMortgageValue(IAsset asset)
        {
            return asset.Value / 2;
        }

        public int GetUnmortgageCost(IAsset asset)
        {
            return (int)(GetMortgageValue(asset) * 1.1);
        }
    }
}