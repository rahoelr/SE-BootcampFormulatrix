namespace MonopolyBackend.Services
{
    using MonopolyBackend.Enums;
    using MonopolyBackend.Interfaces;
    using MonopolyBackend.Models;
    using MonopolyBackend.Common;
    using MonopolyBackend.Structs;
    using System.Collections.Generic;
    using MonopolyBackend.Services.Results;

    public class GameService
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
        public event Action<string>? OnMessage;
        public event Action<IPlayer, int, int>? OnDiceRolled;
        public event Action<IPlayer, ITile>? OnPlayerMoved;
        public event Action<IPlayer, IAsset>? OnPropertyBought;
        public event Action<IPlayer, int>? OnRentPaid;
        public event Action<ICard>? OnCardDrawn;
        public event Action<IPlayer>? OnPlayerBankrupt;
        public event Action<IPlayer>? OnPlayerWins;

        private const int GO_SALARY = 200;
        private const int JAIL_POSITION = 10;
        private const int JAIL_FEE = 50;
        private const int TAX_AMOUNT = 200;
        private const int LUXURY_TAX = 100;

        public GameService(IBoard board, List<IPlayer> players, List<IDice> dices, IDecks communityChestDeck, IDecks chanceDeck, Dictionary<ITile, IAsset?> tileAssets)
        {
            Board = board;
            Players = players;
            Dices = dices;
            CommunityChestDeck = communityChestDeck;
            ChanceDeck = chanceDeck;
            TileAssets = tileAssets;
            PlayerAssets = new Dictionary<IPlayer, List<IAsset>>();
            PlayerMoney = new Dictionary<IPlayer, List<IMoney>>();
            _playerJailTurns = new Dictionary<IPlayer, int>();
            _playerGetOutOfJailCards = new Dictionary<IPlayer, int>();
            CurrentTurn = 0;
            IsGameOver = false;
            Winner = null;

            foreach (var player in Players)
            {
                PlayerAssets[player] = new List<IAsset>();
                PlayerMoney[player] = new List<IMoney> { new Money(1500) }; // Initial money
                _playerJailTurns[player] = 0;
                _playerGetOutOfJailCards[player] = 0;
            }
        }

        public void StartGame()
        {
            OnMessage?.Invoke("Game dimulai! Gas bro!");
            OnMessage?.Invoke($"Pemain : {string.Join(", ", Players.Select(p => p.Name))}");
            OnMessage?.Invoke($"Giliran : {CurrentPlayer.Name}");
        }

        // ===== MAIN GAME LOOP - SINGLE TURN =====
        public void PlayTurn()
        {
            var currentPlayer = CurrentPlayer;

            // Skip bankrupt players
            if (currentPlayer.PlayerState == PlayerState.Bankrupt)
            {
                NextTurn();
                return;
            }

            // Display current game state
            _view.ClearScreen();
            _view.DrawBoard(Board, Players);

            var playerMoneyDict = new Dictionary<IPlayer, int>();
            foreach (var player in Players)
            {
                playerMoneyDict[player] = GetPlayerMoney(player);
            }

            _view.ShowAllPlayersInfo(Players, playerMoneyDict);
            _view.ShowPlayerInfo(currentPlayer, GetPlayerMoney(currentPlayer));
            _view.ShowTurnHeader(currentPlayer.Name);

            // Handle jail
            if (currentPlayer.PlayerState == PlayerState.InJail)
            {
                HandleJailOptions();
                if (currentPlayer.PlayerState == PlayerState.InJail)
                {
                    _view.WaitForKeyPress();
                    NextTurn();
                    return;
                }
            }

            // Roll dice and move
            bool rolled = false;
            bool canRollAgain = false;
            int consecutiveDoubles = 0;

            do
            {
                if (!rolled || canRollAgain)
                {
                    _view.ShowMenu("Aksi", new List<string>
                    {
                        "Lempar Dadu",
                        "Lihat Properti",
                        "Kelola Properti",
                        "Berdagang",
                        "Akhiri Giliran"
                    });

                    int choice = _view.GetPlayerChoice(5);

                    switch (choice)
                    {
                        case 1:
                            int dice1, dice2;
                            (dice1, dice2) = RollDices();
                            rolled = true;

                            if (dice1 == dice2)
                            {
                                consecutiveDoubles++;
                                if (consecutiveDoubles >= 3)
                                {
                                    _view.ShowWarning("Tiga kali ganda berturut-turut! Masuk penjara!");
                                    SendToJail();
                                    canRollAgain = false;
                                }
                                else
                                {
                                    canRollAgain = true;
                                }
                            }
                            else
                            {
                                canRollAgain = false;
                            }

                            if (currentPlayer.PlayerState != PlayerState.InJail)
                            {
                                MovePlayer(dice1 + dice2);
                                OnLand();

                                // Handle property purchase
                                OfferPropertyPurchase();
                            }
                            break;

                        case 2:
                            ShowPlayerProperties();
                            break;

                        case 3:
                            ManagePlayerProperties();
                            break;

                        case 4:
                            TradeFlow();
                            break;

                        case 5:
                            rolled = true;
                            canRollAgain = false;
                            break;
                    }
                }
            } while (canRollAgain && currentPlayer.PlayerState != PlayerState.InJail && !IsGameOver);

            if (!IsGameOver)
            {
                // Post-turn actions
                HandleNegativeBalance();
                _view.WaitForKeyPress();
                NextTurn();
            }
        }

        private void TradeFlow()
        {
            var currentPlayer = CurrentPlayer;
            var otherPlayers = Players
                .Where(p => p != currentPlayer && p.PlayerState != PlayerState.Bankrupt)
                .ToList();

            if (otherPlayers.Count == 0)
            {
                _view.ShowMessage("Tidak ada pemain lain untuk berdagang.");
                _view.WaitForKeyPress();
                return;
            }

            _view.ShowMessage("\n=== Perdagangan ===");

            var targetPlayer = _view.SelectPlayer(
                otherPlayers,
                "Pilih pemain untuk berdagang:",
                p => $"{p.Name} - ${GetPlayerMoney(p)} - {p.Assets.Count} properti"
            );

            if (targetPlayer == null)
            {
                return;
            }

            // Get properties to offer
            List<IAsset> offeredProperties = new List<IAsset>();
            if (currentPlayer.Assets.Count > 0)
            {
                offeredProperties = _view.SelectMultipleFromPropertyList(
                    currentPlayer.Assets.ToList(),
                    $"Pilih properti Anda untuk ditawarkan",
                    a => a.Name
                );
            }
            else
            {
                _view.ShowMessage($"{currentPlayer.Name} tidak memiliki properti.");
            }

            int offeredMoney = _view.GetMoneyAmount("Masukkan jumlah uang untuk ditawarkan: $");

            // Get properties to request
            List<IAsset> requestedProperties = new List<IAsset>();
            if (targetPlayer.Assets.Count > 0)
            {
                requestedProperties = _view.SelectMultipleFromPropertyList(
                    targetPlayer.Assets.ToList(),
                    $"Pilih properti {targetPlayer.Name} yang Anda inginkan",
                    a => a.Name
                );
            }
            else
            {
                _view.ShowMessage($"{targetPlayer.Name} tidak memiliki properti.");
            }

            int requestedMoney = _view.GetMoneyAmount("Masukkan jumlah uang yang diminta: $");

            // Show trade summary
            _view.ShowTradeOffer(currentPlayer, targetPlayer, offeredProperties, offeredMoney, requestedProperties, requestedMoney);

            if (_view.GetYesNo($"Apakah {targetPlayer.Name} menerima perdagangan ini?"))
            {
                PlayerProposeTrade(currentPlayer, targetPlayer, offeredProperties, offeredMoney, requestedProperties, requestedMoney);
            }
            else
            {
                _view.ShowMessage("Perdagangan ditolak.");
            }

            _view.WaitForKeyPress();
        }
        public void NextTurn()
        {
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

        private void HandleNegativeBalance()
        {
            var currentPlayer = CurrentPlayer;
            int playerMoney = GetPlayerMoney(currentPlayer).Data;

            // Check if current player is bankrupt
            if (playerMoney < 0)
            {
                _view.ShowWarning($"{currentPlayer.Name} memiliki saldo negatif!");

                // Allow player to mortgage properties or sell houses
                while (GetPlayerMoney(currentPlayer).Data < 0 && currentPlayer.Assets.Count > 0)
                {
                    _view.ShowMessage($"Saldo saat ini: ${GetPlayerMoney(currentPlayer).Data}");
                    _view.ShowMenu("Anda harus mengumpulkan dana!", new List<string>
                    {
                        "Jual Rumah",
                        "Mortgage Properti",
                        "Nyatakan Bangkrut"
                    });

                    int choice = _view.GetPlayerChoice(3);
                    switch (choice)
                    {
                        case 1:
                            SellHouseFlow();
                            break;
                        case 2:
                            MortgageFlow();
                            break;
                        case 3:
                            CheckIsBankrupt(currentPlayer);
                            return;
                    }
                }

                var getPlayerMoney = GetPlayerMoney(currentPlayer).Data;
                if (getPlayerMoney < 0)
                {
                    CheckIsBankrupt(currentPlayer);
                }
            }
        }

        public ServiceResult<int> GetPlayerMoney(IPlayer player)
        {
            int getPlayerMoneyResult = PlayerMoney[player].Sum(m => m.Balance);
            return ServiceResult<int>.Success(getPlayerMoneyResult);
        }

        public List<IPlayer> GetActivePlayers()
        {
            return Players.Where(p => p.PlayerState != PlayerState.Bankrupt).ToList();
        }

        public void GetAndApplyDeck(IDecks deck)
        {
            var cardResult = DrawCardFromDeck(deck);
            if (!cardResult.IsSuccess || cardResult.Data == null)
            {
                OnMessage?.Invoke("Gagal mengambil kartu dari deck.");
                return;
            }

            ApplyCardEffect(cardResult.Data);
        }

        public void SendToJail()
        {
            CurrentPlayer.PathIndex = JAIL_POSITION;
            CurrentPlayer.CurrentTile = Board.Path[JAIL_POSITION];
            CurrentPlayer.PlayerState = PlayerState.InJail;
            _playerJailTurns[CurrentPlayer] = 0;
            OnMessage?.Invoke($"{CurrentPlayer.Name} masuk Penjara!");
        }

        public ServiceResult<ITile> MovePlayer(int steps)
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

            return ServiceResult<ITile>.Success(CurrentPlayer.CurrentTile);
        }

        public void ApplyCardEffect(ICard card)
        {
            switch (card.CardEffect)
            {
                case CardEffect.ReceiveMoney:
                    AddMoney(CurrentPlayer, card.Value);
                    break;

                case CardEffect.PayMoney:
                    var subtractResult = SubtractMoney(CurrentPlayer, card.Value);
                    if (subtractResult.IsSuccess)
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

        private void HandleJailOptions()
        {
            var currentPlayer = CurrentPlayer;

            // Increment jail turns dan cek apakah sudah 3 giliran
            var handleJailTurnResult = HandleJailTurn();
            bool canChoose = handleJailTurnResult.IsSuccess && handleJailTurnResult.Data;
            if (!canChoose)
            {
                // Sudah 3 giliran atau state bukan InJail, sudah dihandle
                return;
            }

            var getJailReturns = GetJailTurns(currentPlayer);
            int jailTurns = getJailReturns.Data;
            _view.ShowWarning($"{currentPlayer.Name} di Penjara! (Giliran {jailTurns}/3)");

            var options = new List<string>
            {
                "Coba lempar ganda",
                "Bayar $50 untuk keluar"
            };

            if ()
            {
                options.Add("Gunakan kartu Bebas Penjara");
            }

            _view.ShowMenu("Opsi Penjara", options);
            int choice = _view.GetPlayerChoice(options.Count);

            switch (choice)
            {
                case 1:
                    TryRollDoublesInJail();
                    break;
                case 2:
                    PayJailFee();
                    break;
                case 3:
                    UseGetOutOfJailCard();
                    break;
            }
        }

        public ServiceResult<bool> HandleJailTurn()
        {
            if (CurrentPlayer.PlayerState != PlayerState.InJail)
                return ServiceResult<bool>.Fail(
                    new ServiceError(ErrorType.Validation, "Player is not in jail.")
                );

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
            return ServiceResult<bool>.Success(true);
        }

        public ServiceResult<bool> PayJailFee()
        {
            if (CurrentPlayer.PlayerState != PlayerState.InJail)
                return ServiceResult<bool>.Fail(
                    new ServiceError(ErrorType.Validation, "Player is not in jail.")
                );

            var resultSubtract = SubtractMoney(CurrentPlayer, JAIL_FEE);
            if (!resultSubtract.IsSuccess)
                return ServiceResult<bool>.Fail(
                    new ServiceError(ErrorType.Validation, "Insufficient funds to pay jail fee.")
                );

            CurrentPlayer.PlayerState = PlayerState.Normal;
            _playerJailTurns[CurrentPlayer] = 0;
            OnMessage?.Invoke($"{CurrentPlayer.Name} keluar dari penjara.");
            return ServiceResult<bool>.Success(true);
        }

        public ServiceResult<bool> UseGetOutOfJailCard()
        {
            if (CurrentPlayer.PlayerState != PlayerState.InJail)
                return ServiceResult<bool>.Fail(
                    new ServiceError(ErrorType.Validation, "Player is not in jail.")
                );

            if (_playerGetOutOfJailCards[CurrentPlayer] <= 0)
            {
                OnMessage?.Invoke($"{CurrentPlayer.Name} tidak memiliki kartu bebas penjara.");
                return ServiceResult<bool>.Fail(
                    new ServiceError(ErrorType.Validation, "Player does not have a Get Out of Jail card.")
                );
            }

            _playerGetOutOfJailCards[CurrentPlayer]--;
            CurrentPlayer.PlayerState = PlayerState.Normal;
            _playerJailTurns[CurrentPlayer] = 0;
            OnMessage?.Invoke($"{CurrentPlayer.Name} menggunakan kartu bebas penjara!");
            return ServiceResult<bool>.Success(true);
        }

        public ServiceResult<int> GetJailTurns(IPlayer player)
        {
            int result = _playerJailTurns.ContainsKey(player) ? _playerJailTurns[player] : 0;
            return ServiceResult<int>.Success(result);
        }

        public ServiceResult<bool> HasGetOutOfJailCard(IPlayer player)
        {
            bool result = _playerGetOutOfJailCards.ContainsKey(player) &&
                          _playerGetOutOfJailCards[player] > 0;
            return ServiceResult<bool>.Success(result);
        }

        public ServiceResult<RollDicesResults> RollDices()
        {
            Random rand = new Random();
            int dice1 = rand.Next(1, 7);
            int dice2 = rand.Next(1, 7);

            var result = new RollDicesResults(dice1, dice2);
            int total = dice1 + dice2;

            OnDiceRolled?.Invoke(CurrentPlayer, dice1, dice2);
            OnMessage?.Invoke($"{CurrentPlayer.Name} melempar dadu dan mendapatkan {dice1} dan {dice2} dengan total {total}");

            return ServiceResult<RollDicesResults>.Success(result);
        }

        public ServiceResult<bool> TryRollDoublesInJail()
        {
            if (CurrentPlayer.PlayerState != PlayerState.InJail)
                return ServiceResult<bool>.Fail(
                    new ServiceError(ErrorType.Validation, "Player is not in jail.")
                );

            var rollResult = RollDices();
            if (!rollResult.IsSuccess || rollResult.Data == null)
                return ServiceResult<bool>.Fail(
                    new ServiceError(ErrorType.Validation, "Failed to roll dice.")
                );

            var diceData = rollResult.Data;

            if (diceData.Dice1 == diceData.Dice2)
            {
                // Berhasil roll ganda - keluar dari penjara
                CurrentPlayer.PlayerState = PlayerState.Normal;
                _playerJailTurns[CurrentPlayer] = 0;
                OnMessage?.Invoke($"{CurrentPlayer.Name} melempar ganda dan keluar dari penjara!");

                // Pindahkan pemain
                MovePlayer(diceData.Dice1 + diceData.Dice2);
                OnLand();
                return ServiceResult<bool>.Success(true);
            }
            else
            {
                OnMessage?.Invoke($"{CurrentPlayer.Name} tidak mendapat ganda. Tetap di penjara.");
                return ServiceResult<bool>.Success(false);
            }
        }

        private void OfferPropertyPurchase()
        {
            var tile = CurrentPlayer.CurrentTile;
            if (tile == null) return;

            var asset = TileAssets.ContainsKey(tile) ? TileAssets[tile] : null;

            if (asset == null || asset.Owner != null)
                return;

            var player = CurrentPlayer;

            _view.ShowPropertyDetails(asset);

            int playerMoney = GetPlayerMoney(player);
            if (playerMoney >= asset.Value)
            {
                if (_view.GetYesNo($"Beli {asset.Name} seharga ${asset.Value}?"))
                {
                    PlayerBuyAsset(asset);
                }
            }
            else
            {
                _view.ShowWarning($"Uang tidak cukup untuk membeli {asset.Name}.");
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

        private void ShowPlayerProperties()
        {
            var player = CurrentPlayer;

            if (player.Assets.Count == 0)
            {
                _view.ShowMessage("Anda tidak memiliki properti.");
                _view.WaitForKeyPress();
                return;
            }

            int? selectedIndex = _view.SelectFromPropertyList(
                player.Assets.ToList(),
                "Properti Anda",
                asset =>
                {
                    string status = asset.AssetCondition == AssetCondition.Mortgage ? " [MORTGAGE]" : "";
                    string houses = asset.AmountHouse > 0 ? $" - {asset.AmountHouse} rumah" : "";
                    return $"{asset.Name} - ${asset.Value}{status}{houses}";
                }
            );

            if (selectedIndex.HasValue)
            {
                _view.ShowPropertyDetails(player.Assets[selectedIndex.Value]);
            }
            _view.WaitForKeyPress();
        }

        private void ManagePlayerProperties()
        {
            var player = CurrentPlayer;

            if (player.Assets.Count == 0)
            {
                _view.ShowMessage("Anda tidak memiliki properti.");
                _view.WaitForKeyPress();
                return;
            }

            _view.ShowMenu("Kelola Properti", new List<string>
            {
                "Bangun Rumah",
                "Jual Rumah",
                "Mortgage Properti",
                "Unmortgage Properti",
                "Kembali"
            });

            int choice = _view.GetPlayerChoice(5);

            switch (choice)
            {
                case 1:
                    BuildHouseFlow();
                    break;
                case 2:
                    SellHouseFlow();
                    break;
                case 3:
                    MortgageFlow();
                    break;
                case 4:
                    UnmortgageFlow();
                    break;
                case 5:
                    return;
            }
        }

        // ===== BUILD HOUSE =====
        private void BuildHouseFlow()
        {
            var player = CurrentPlayer;
            var buildableProperties = player.Assets
                .Where(a => a.TypeAsset == TypeAsset.RealEstate &&
                            a.AmountHouse < 5 &&
                            a.AssetCondition == AssetCondition.Normal)
                .ToList();

            if (buildableProperties.Count == 0)
            {
                _view.ShowMessage("Tidak ada properti untuk dibangun.");
                _view.WaitForKeyPress();
                return;
            }

            int? selectedIndex = _view.SelectFromPropertyList(
                buildableProperties,
                "Bangun Rumah",
                asset =>
                {
                    int houseCost = asset.Value / 2;
                    return $"{asset.Name} - Biaya rumah: ${houseCost} - Saat ini: {asset.AmountHouse} rumah";
                }
            );

            if (selectedIndex.HasValue)
            {
                PlayerAddHouse(buildableProperties[selectedIndex.Value]);
            }
            _view.WaitForKeyPress();
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

        private void SellHouseFlow()
        {
            var player = CurrentPlayer;
            var sellableProperties = player.Assets
                .Where(a => a.AmountHouse > 0)
                .ToList();

            if (sellableProperties.Count == 0)
            {
                _view.ShowMessage("Tidak ada rumah untuk dijual.");
                _view.WaitForKeyPress();
                return;
            }

            int? selectedIndex = _view.SelectFromPropertyList(
                sellableProperties,
                "Jual Rumah",
                asset =>
                {
                    int sellPrice = asset.Value / 4;
                    return $"{asset.Name} - Rumah: {asset.AmountHouse} - Harga jual: ${sellPrice}";
                }
            );

            if (selectedIndex.HasValue)
            {
                PlayerSellHouse(sellableProperties[selectedIndex.Value]);
            }
            _view.WaitForKeyPress();
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

        private void MortgageFlow()
        {
            var player = CurrentPlayer;
            var mortgageableProperties = player.Assets
                .Where(a => a.AssetCondition == AssetCondition.Normal && a.AmountHouse == 0)
                .ToList();

            if (mortgageableProperties.Count == 0)
            {
                _view.ShowMessage("Tidak ada properti untuk di-mortgage.");
                _view.WaitForKeyPress();
                return;
            }

            int? selectedIndex = _view.SelectFromPropertyList(
                mortgageableProperties,
                "Mortgage Properti",
                asset =>
                {
                    int mortgageValue = asset.Value / 2;
                    return $"{asset.Name} - Nilai mortgage: ${mortgageValue}";
                }
            );

            if (selectedIndex.HasValue)
            {
                PlayerMortgageAsset(player, mortgageableProperties[selectedIndex.Value]);
            }
            _view.WaitForKeyPress();
        }


        public bool PlayerMortgageAsset(IPlayer player, IAsset asset)
        {
            if (asset.Owner != player)
            {
                OnMessage?.Invoke("Pemain tidak memiliki properti ini.");
                return false;
            }

            if (asset.AssetCondition == AssetCondition.Mortgage)
            {
                OnMessage?.Invoke("Properti sudah di-mortgage.");
                return false;
            }

            if (asset.AmountHouse > 0)
            {
                OnMessage?.Invoke("Harus jual semua rumah sebelum mortgage.");
                return false;
            }

            asset.AssetCondition = AssetCondition.Mortgage;
            int mortgageValue = GetMortgageValue(asset);
            AddMoney(player, mortgageValue);
            OnMessage?.Invoke($"{player.Name} mortgage {asset.Name} seharga ${mortgageValue}.");
            return true;
        }

        private void UnmortgageFlow()
        {
            var player = CurrentPlayer;
            var mortgagedProperties = player.Assets
                .Where(a => a.AssetCondition == AssetCondition.Mortgage)
                .ToList();

            if (mortgagedProperties.Count == 0)
            {
                _view.ShowMessage("Tidak ada properti yang di-mortgage.");
                _view.WaitForKeyPress();
                return;
            }

            int? selectedIndex = _view.SelectFromPropertyList(
                mortgagedProperties,
                "Unmortgage Properti",
                asset =>
                {
                    int unmortgageValue = (asset.Value / 2) + ((asset.Value / 2) / 10);
                    return $"{asset.Name} - Biaya unmortgage: ${unmortgageValue}";
                }
            );

            if (selectedIndex.HasValue)
            {
                PlayerUnmortgageAsset(player, mortgagedProperties[selectedIndex.Value]);
            }
            _view.WaitForKeyPress();
        }

        public bool PlayerUnmortgageAsset(IPlayer player, IAsset asset)
        {
            if (asset.Owner != player)
            {
                OnMessage?.Invoke("Pemain tidak memiliki properti ini.");
                return false;
            }

            if (asset.AssetCondition != AssetCondition.Mortgage)
            {
                OnMessage?.Invoke("Properti tidak di-mortgage.");
                return false;
            }

            int unmortgageValue = GetUnmortgageCost(asset);
            if (!SubtractMoney(player, unmortgageValue))
            {
                OnMessage?.Invoke($"Uang tidak cukup untuk unmortgage. Butuh ${unmortgageValue}.");
                return false;
            }

            asset.AssetCondition = AssetCondition.Normal;
            OnMessage?.Invoke($"{player.Name} unmortgage {asset.Name} seharga ${unmortgageValue}.");
            return true;
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
                    var subtractTaxResult = SubtractMoney(CurrentPlayer, taxAmount);
                    if (!subtractTaxResult.IsSuccess)
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
                    int rent = CalculateRent(asset).Data;

                    var subtractResult = SubtractMoney(CurrentPlayer, rent);
                    if (subtractResult.IsSuccess)
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

        private ServiceResult<int> CountSameTypeAssets(IPlayer owner, IAsset asset)
        {
            int result = owner.Assets.Count(a => a.TypeAsset == asset.TypeAsset);
            return ServiceResult<int>.Success(result);
        }

        private ServiceResult<int> CalculateRent(IAsset asset)
        {
            int sameTypeCount = CountSameTypeAssets(asset.Owner!, asset).Data;

            // Utility (Perusahaan Listrik/Air) - Fixed price
            if (asset.TypeAsset == TypeAsset.PublicService)
            {
                // Jika punya 1 utility: $25
                // Jika punya 2 utility: $50
                var result = sameTypeCount == 1 ? 25 : 50;
                return ServiceResult<int>.Success(result);
            }

            // Railroad (Stasiun) - Fixed price berdasarkan jumlah stasiun
            if (asset.TypeAsset == TypeAsset.Railroad)
            {
                return ServiceResult<int>.Success(sameTypeCount switch
                {
                    1 => 25,
                    2 => 50,
                    3 => 100,
                    4 => 200,
                    _ => 25
                });
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
                return ServiceResult<int>.Success(asset.AmountHouse switch
                {
                    1 => baseRent * 5,
                    2 => baseRent * 15,
                    3 => baseRent * 45,
                    4 => baseRent * 80,
                    5 => baseRent * 100,  // Hotel
                    _ => baseRent
                });
            }

            // Return base rent
            return ServiceResult<int>.Success(baseRent);
        }


        public void MovePlayerToPosition(int position)
        {
            int oldPosition = CurrentPlayer.PathIndex;

            // Check if passed GO
            if (position < oldPosition)
            {
                AddMoney(CurrentPlayer, GO_SALARY);
                OnMessage?.Invoke($"{CurrentPlayer.Name} melewati MULAI dan menerima ${GO_SALARY}!");
            }

            CurrentPlayer.PathIndex = position;
            CurrentPlayer.CurrentTile = Board.Path[position];
            OnPlayerMoved?.Invoke(CurrentPlayer, CurrentPlayer.CurrentTile);
        }

        public ServiceResult<ICard> DrawCardFromDeck(IDecks deck)
        {
            if (deck == null)
                throw new ArgumentNullException(nameof(deck));

            var cards = deck.Cards;
            if (cards == null || cards.Count == 0)
                throw new InvalidOperationException("Deck kosong");

            // Ambil kartu pertama dan pindahkan ke belakang (shuffle)
            var card = cards[0];
            cards.RemoveAt(0);
            cards.Add(card);

            OnCardDrawn?.Invoke(card);
            OnMessage?.Invoke($"Kartu: {card.Name} - {card.Description}");

            return ServiceResult<ICard>.Success(card);
        }


        public ServiceResult<bool> CheckIsBankrupt(IPlayer player)
        {
            var totalValue = CalculatePlayerTotalAssetsValue(player);
            var playerMoney = GetPlayerMoney(player);

            if (playerMoney.Data + totalValue.Data < 0)
            {
                player.PlayerState = PlayerState.Bankrupt;
                OnPlayerBankrupt?.Invoke(player);
                OnMessage?.Invoke($"{player.Name} BANGKRUT!");

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

                return ServiceResult<bool>.Success(true);
            }

            return ServiceResult<bool>.Success(false);
        }

        public ServiceResult<int> CalculatePlayerTotalAssetsValue(IPlayer player)
        {
            int total = 0;
            foreach (var asset in player.Assets)
            {
                if (asset.AssetCondition == AssetCondition.Mortgage)
                {
                    var mortgageResult = GetMortgageValue(asset);
                    if (!mortgageResult.IsSuccess)
                    {
                        return ServiceResult<int>.Fail(mortgageResult.Error!);
                    }
                    total += mortgageResult.Data;
                }
                else
                {
                    total += asset.Value;
                }
                total += asset.AmountHouse * (asset.Value / 2);
            }
            return ServiceResult<int>.Success(total);
        }

        public ServiceResult<int> GetMortgageValue(IAsset asset)
        {
            var mortageResult = asset.Value / 2;
            return ServiceResult<int>.Success(mortageResult);
        }

        public ServiceResult<int> GetUnmortgageCost(IAsset asset)
        {
            var mortgageResult = GetMortgageValue(asset);

            if (!mortgageResult.IsSuccess)
            {
                return ServiceResult<int>.Fail(mortgageResult.Error!);
            }

            int unmortgageCost = (int)(mortgageResult.Data * 1.1);

            return ServiceResult<int>.Success(unmortgageCost);
        }

        public ServiceResult<bool> AddMoney(IPlayer player, int amount)
        {
            if (amount <= 0)
                return ServiceResult<bool>.Fail(
                    new ServiceError(ErrorType.Validation, "Amount must be greater than zero.")
                );

            var money = new Money(amount);
            PlayerMoney[player].Add(money);
            OnMessage?.Invoke($"{player.Name} menerima ${amount}");
            return ServiceResult<bool>.Success(true);
        }

        public ServiceResult<bool> SubtractMoney(IPlayer player, int amount)
        {
            if (amount <= 0)
                return ServiceResult<bool>.Fail(
                    new ServiceError(ErrorType.Validation, "Amount must be greater than zero.")
                );

            // Check if player has enough money
            int currentMoney = PlayerMoney[player].Sum(m => m.Balance);
            if (currentMoney < amount)
            {
                OnMessage?.Invoke($"{player.Name} tidak punya cukup uang. Dibutuhkan ${amount}, punya ${currentMoney}");
                return ServiceResult<bool>.Fail(
                    new ServiceError(ErrorType.Validation, "Insufficient funds.")
                );
            }

            // Deduct money
            int remaining = amount;
            var moneyList = PlayerMoney[player].OrderByDescending(m => m.Balance).ToList();

            foreach (var money in moneyList)
            {
                if (remaining <= 0) break;

                if (money.Balance <= remaining)
                {
                    // Uang ini habis dipakai
                    remaining -= money.Balance;
                    PlayerMoney[player].Remove(money);
                }
                else
                {
                    // Uang ini cukup untuk sisa pembayaran, kurangi balance-nya
                    money.Balance -= remaining;
                    remaining = 0;
                }
            }

            OnMessage?.Invoke($"{player.Name} membayar ${amount}");
            return ServiceResult<bool>.Success(true);
        }

    }
}