namespace MonopolyBackend.Services
{
    using MonopolyBackend.Enums;
    using MonopolyBackend.Interfaces;
    using MonopolyBackend.Models;
    using MonopolyBackend.Models.Results;
    using MonopolyBackend.Common;
    using MonopolyBackend.Structs;
    using System.Collections.Generic;
    using MonopolyBackend.DTOs.Requests; // Only for input parameters (TradeRequest)
    using PlayerStateEnum = MonopolyBackend.Enums.PlayerState;

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
        private Dictionary<IPlayer, bool> _hasRolledThisTurn { get; set; }

        private ServiceResult<IPlayer> ValidatePlayerTurn(string playerName)
        {
            var player = Players.FirstOrDefault(p => p.Name == playerName);
            if (player == null)
                return ServiceResult<IPlayer>.Fail(
                    new ServiceError(ErrorType.Validation, $"Player '{playerName}' not found.")
                );

            if (CurrentPlayer != player)
                return ServiceResult<IPlayer>.Fail(
                    new ServiceError(ErrorType.Validation, $"It's not {playerName}'s turn. Current player is {CurrentPlayer.Name}.")
                );

            return ServiceResult<IPlayer>.Success(player);
        }

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
            _hasRolledThisTurn = new Dictionary<IPlayer, bool>();
            CurrentTurn = 0;
            IsGameOver = false;
            Winner = null;

            foreach (var player in Players)
            {
                PlayerAssets[player] = new List<IAsset>();
                PlayerMoney[player] = new List<IMoney> { new Money(1500) }; // Initial money
                _playerJailTurns[player] = 0;
                _playerGetOutOfJailCards[player] = 0;
                _hasRolledThisTurn[player] = false;
            }
        }

        public void StartGame()
        {
        }
        
        public void NextTurn()
        {
            do
            {
                CurrentTurn++;
            } while (CurrentPlayer.PlayerState == PlayerStateEnum.Bankrupt && GetActivePlayers().Count > 1);

            var activePlayers = GetActivePlayers();
            if (activePlayers.Count == 1)
            {
                IsGameOver = true;
                Winner = activePlayers[0];
                return;
            }

            // Reset roll flag untuk pemain selanjutnya
            _hasRolledThisTurn[CurrentPlayer] = false;
        }

        // HandleNegativeBalance() deleted - bankruptcy handled via API

        public ServiceResult<int> GetPlayerMoney(IPlayer player)
        {
            int getPlayerMoneyResult = PlayerMoney[player].Sum(m => m.Balance);
            return ServiceResult<int>.Success(getPlayerMoneyResult);
        }

        public List<IPlayer> GetActivePlayers()
        {
            return Players.Where(p => p.PlayerState != PlayerStateEnum.Bankrupt).ToList();
        }

        public void GetAndApplyDeck(IDecks deck)
        {
            var cardResult = DrawCardFromDeck(deck);
            if (!cardResult.IsSuccess || cardResult.Data == null)
            {
                return;
            }

            ApplyCardEffect(cardResult.Data);
        }

        public void SendToJail()
        {
            CurrentPlayer.PathIndex = JAIL_POSITION;
            CurrentPlayer.CurrentTile = Board.Path[JAIL_POSITION];
            CurrentPlayer.PlayerState = PlayerStateEnum.InJail;
            _playerJailTurns[CurrentPlayer] = 0;
        }

        public ServiceResult<ITile> MovePlayer(int steps)
        {
            int oldPosition = CurrentPlayer.PathIndex;
            int newPosition = (oldPosition + steps) % Board.Path.Count;

            // Check if passed GO
            if (newPosition < oldPosition && steps > 0)
            {
                AddMoney(CurrentPlayer, GO_SALARY);
            }

            CurrentPlayer.PathIndex = newPosition;
            CurrentPlayer.CurrentTile = Board.Path[newPosition];

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

        // HandleJailOptions() deleted - use ExecutePayJailFee, ExecuteUseJailCard, ExecuteTryRollDoublesInJail

        public ServiceResult<bool> HandleJailTurn()
        {
            if (CurrentPlayer.PlayerState != PlayerStateEnum.InJail)
                return ServiceResult<bool>.Fail(
                    new ServiceError(ErrorType.Validation, "Player is not in jail.")
                );

            if (!_playerJailTurns.ContainsKey(CurrentPlayer))
                _playerJailTurns[CurrentPlayer] = 0;

            _playerJailTurns[CurrentPlayer]++;
            int jailTurns = _playerJailTurns[CurrentPlayer];

            // After 3 turns, must pay
            if (jailTurns >= 3)
            {
                return PayJailFee();
            }

            // Options: Pay $50, Use Get Out of Jail card, or try to roll doubles
            return ServiceResult<bool>.Success(true);
        }

        public ServiceResult<bool> PayJailFee()
        {
            if (CurrentPlayer.PlayerState != PlayerStateEnum.InJail)
                return ServiceResult<bool>.Fail(
                    new ServiceError(ErrorType.Validation, "Player is not in jail.")
                );

            var resultSubtract = SubtractMoney(CurrentPlayer, JAIL_FEE);
            if (!resultSubtract.IsSuccess)
                return ServiceResult<bool>.Fail(
                    new ServiceError(ErrorType.Validation, "Insufficient funds to pay jail fee.")
                );

            CurrentPlayer.PlayerState = PlayerStateEnum.Normal;
            _playerJailTurns[CurrentPlayer] = 0;
            return ServiceResult<bool>.Success(true);
        }

        public ServiceResult<bool> UseGetOutOfJailCard()
        {
            if (CurrentPlayer.PlayerState != PlayerStateEnum.InJail)
                return ServiceResult<bool>.Fail(
                    new ServiceError(ErrorType.Validation, "Player is not in jail.")
                );

            if (_playerGetOutOfJailCards[CurrentPlayer] <= 0)
            {
                return ServiceResult<bool>.Fail(
                    new ServiceError(ErrorType.Validation, "Player does not have a Get Out of Jail card.")
                );
            }

            _playerGetOutOfJailCards[CurrentPlayer]--;
            CurrentPlayer.PlayerState = PlayerStateEnum.Normal;
            _playerJailTurns[CurrentPlayer] = 0;
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

        public ServiceResult<DiceRoll> RollDices()
        {
            Random rand = new Random();
            int dice1 = rand.Next(1, 7);
            int dice2 = rand.Next(1, 7);

            var result = new DiceRoll
            {
                Dice1 = dice1,
                Dice2 = dice2
            };

            return ServiceResult<DiceRoll>.Success(result);
        }

        public ServiceResult<bool> TryRollDoublesInJail()
        {
            if (CurrentPlayer.PlayerState != PlayerStateEnum.InJail)
                return ServiceResult<bool>.Fail(
                    new ServiceError(ErrorType.Validation, "Player is not in jail.")
                );

            var rollResult = RollDices();
            if (!rollResult.IsSuccess || rollResult.Data == null)
                return ServiceResult<bool>.Fail(
                    new ServiceError(ErrorType.Validation, "Failed to roll dice.")
                );

            var diceData = rollResult.Data;

            if (diceData.IsDouble)
            {
                // Berhasil roll ganda - keluar dari penjara
                CurrentPlayer.PlayerState = PlayerStateEnum.Normal;
                _playerJailTurns[CurrentPlayer] = 0;

                // Pindahkan pemain
                MovePlayer(diceData.Total);
                OnLand();
                return ServiceResult<bool>.Success(true);
            }
            else
            {
                return ServiceResult<bool>.Success(false);
            }
        }

        // OfferPropertyPurchase() deleted - use ExecuteBuyProperty() API method instead

        public ServiceResult<bool> PlayerBuyAsset(IAsset asset)
        {
            if (asset.Owner != null)
            {
                return ServiceResult<bool>.Fail(
                    new ServiceError(ErrorType.Validation, "Properti ini sudah dimiliki.")
                );
            }

            if (!SubtractMoney(CurrentPlayer, asset.Value).IsSuccess)
            {
                return ServiceResult<bool>.Fail(
                    new ServiceError(ErrorType.Validation, $"{CurrentPlayer.Name} tidak punya cukup uang untuk membeli {asset.Name}.")
                );
            }

            // Set owner dan tambahkan ke assets
            asset.Owner = CurrentPlayer;
            CurrentPlayer.Assets.Add(asset);
            PlayerAssets[CurrentPlayer].Add(asset);

            return ServiceResult<bool>.Success(true);
        }

        // ShowPlayerProperties() deleted - frontend displays player properties via GetGameState() API

        // ManagePlayerProperties() deleted - use specific API endpoints (build-house, sell-house, mortgage, etc.)

        // BuildHouseFlow() deleted - use ExecuteBuildHouse() API method instead

        public ServiceResult<bool> PlayerAddHouse(IAsset asset)
        {
            if (asset.Owner != CurrentPlayer)
            {
                return ServiceResult<bool>.Fail(
                    new ServiceError(ErrorType.Validation, "Anda tidak memiliki properti ini.")
                );
            }

            if (asset.TypeAsset != TypeAsset.RealEstate)
            {
                return ServiceResult<bool>.Fail(
                    new ServiceError(ErrorType.Validation, "Hanya bisa membangun rumah di properti RealEstate.")
                );
            }

            if (asset.AmountHouse >= 5)
            {
                return ServiceResult<bool>.Fail(
                    new ServiceError(ErrorType.Validation, "Maksimum rumah (hotel) sudah dibangun.")
                );
            }

            // Hitung biaya rumah (50% dari nilai properti)
            int houseCost = asset.Value / 2;

            if (!SubtractMoney(CurrentPlayer, houseCost).IsSuccess)
            {
                return ServiceResult<bool>.Fail(
                    new ServiceError(ErrorType.Validation, $"Uang tidak cukup. Rumah berharga ${houseCost}.")
                );
            }

            asset.AmountHouse++;
            string buildingType = asset.AmountHouse == 5 ? "hotel" : "rumah";
            return ServiceResult<bool>.Success(true);
        }

        // SellHouseFlow() deleted - use ExecuteSellHouse() API method instead

        public ServiceResult<bool> PlayerProposeTrade(IPlayer player1, IPlayer player2,
                                List<IAsset> offer1, int money1,
                                List<IAsset> offer2, int money2)
        {
            // Validate ownership
            foreach (var asset in offer1)
            {
                if (asset.Owner != player1)
                {
                    return ServiceResult<bool>.Fail(
                        new ServiceError(ErrorType.Validation, $"{player1.Name} tidak memiliki {asset.Name}.")
                    );
                }
            }

            foreach (var asset in offer2)
            {
                if (asset.Owner != player2)
                {
                    return ServiceResult<bool>.Fail(
                        new ServiceError(ErrorType.Validation, $"{player2.Name} tidak memiliki {asset.Name}.")
                    );
                }
            }

            // Check money using PlayerMoney dictionary
            var player1MoneyResult = GetPlayerMoney(player1);
            var player2MoneyResult = GetPlayerMoney(player2);

            if (!player1MoneyResult.IsSuccess)
                return ServiceResult<bool>.Fail(player1MoneyResult.Error!);

            if (!player2MoneyResult.IsSuccess)
                return ServiceResult<bool>.Fail(player2MoneyResult.Error!);

            if (player1MoneyResult.Data < money1)
            {
                return ServiceResult<bool>.Fail(
                    new ServiceError(ErrorType.Validation, $"{player1.Name} tidak punya ${money1}.")
                );
            }

            if (player2MoneyResult.Data < money2)
            {
                return ServiceResult<bool>.Fail(
                    new ServiceError(ErrorType.Validation, $"{player2.Name} tidak punya ${money2}.")
                );
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

            return ServiceResult<bool>.Success(true);
        }

        public ServiceResult<bool> PlayerSellHouse(IAsset asset)
        {
            if (asset.Owner != CurrentPlayer)
            {
                return ServiceResult<bool>.Fail(
                    new ServiceError(ErrorType.Validation, "Anda tidak memiliki properti ini.")
                );
            }

            if (asset.TypeAsset != TypeAsset.RealEstate)
            {
                return ServiceResult<bool>.Fail(
                    new ServiceError(ErrorType.Validation, "Properti ini tidak memiliki rumah.")
                );
            }

            if (asset.AmountHouse <= 0)
            {
                return ServiceResult<bool>.Fail(
                    new ServiceError(ErrorType.Validation, "Tidak ada rumah untuk dijual.")
                );
            }

            // Jual rumah dengan harga 50% dari harga beli
            int sellPrice = asset.Value / 4;
            asset.AmountHouse--;
            AddMoney(CurrentPlayer, sellPrice);

            string buildingType = asset.AmountHouse == 4 ? "hotel" : "rumah";
            return ServiceResult<bool>.Success(true);
        }

        // MortgageFlow() deleted - use ExecuteMortgage() API method instead


        public ServiceResult<bool> PlayerMortgageAsset(IPlayer player, IAsset asset)
        {
            if (asset.Owner != player)
            {
                return ServiceResult<bool>.Fail(
                    new ServiceError(ErrorType.Validation, "Player does not own this asset.")
                );
            }

            if (asset.AssetCondition == AssetCondition.Mortgage)
            {
                return ServiceResult<bool>.Fail(
                    new ServiceError(ErrorType.Validation, "Asset is already mortgaged.")
                );
            }

            if (asset.AmountHouse > 0)
            {
                return ServiceResult<bool>.Fail(
                    new ServiceError(ErrorType.Validation, "Must sell all houses before mortgaging.")
                );
            }

            asset.AssetCondition = AssetCondition.Mortgage;
            int mortgageValue = GetMortgageValue(asset).Data;
            AddMoney(player, mortgageValue);
            return ServiceResult<bool>.Success(true);
        }

        // UnmortgageFlow() deleted - use ExecuteUnmortgage() API method instead

        public ServiceResult<bool> PlayerUnmortgageAsset(IPlayer player, IAsset asset)
        {
            if (asset.Owner != player)
            {
                return ServiceResult<bool>.Fail(
                    new ServiceError(ErrorType.Validation, "Player does not own this asset.")
                );
            }

            if (asset.AssetCondition != AssetCondition.Mortgage)
            {
                return ServiceResult<bool>.Fail(
                    new ServiceError(ErrorType.Validation, "Asset is not mortgaged.")
                );
            }

            int unmortgageValue = GetUnmortgageCost(asset).Data;
            if (!SubtractMoney(player, unmortgageValue).IsSuccess)
            {
                return ServiceResult<bool>.Fail(
                    new ServiceError(ErrorType.Validation, "Insufficient funds to unmortgage asset.")
                );
            }

            asset.AssetCondition = AssetCondition.Normal;
            return ServiceResult<bool>.Success(true);
        }

        public void OnLand()
        {
            var tile = CurrentPlayer.CurrentTile;
            if (tile == null) return;

            switch (tile.EffectType)
            {
                case EffectType.Go:
                    // Already handled in MovePlayer if passing GO
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
                return;
            }

            var asset = TileAssets[tile]!;

            if (asset.Owner == null)
            {
                // Property available for purchase
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
                    }
                    else
                    {
                        CheckIsBankrupt(CurrentPlayer);
                    }
                }
                else
                {
                }
            }
            else
            {
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
            }

            CurrentPlayer.PathIndex = position;
            CurrentPlayer.CurrentTile = Board.Path[position];
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

            return ServiceResult<ICard>.Success(card);
        }


        public ServiceResult<bool> CheckIsBankrupt(IPlayer player)
        {
            var totalValue = CalculatePlayerTotalAssetsValue(player);
            var playerMoney = GetPlayerMoney(player);

            if (playerMoney.Data + totalValue.Data < 0)
            {
                player.PlayerState = PlayerStateEnum.Bankrupt;

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

            return ServiceResult<bool>.Success(true);
        }

        public ServiceResult<RollDiceResult> ExecuteRollDice(string playerName)
        {
            var validationResult = ValidatePlayerTurn(playerName);
            if (!validationResult.IsSuccess)
                return ServiceResult<RollDiceResult>.Fail(validationResult.Error!);

            if (CurrentPlayer.PlayerState == PlayerStateEnum.InJail)
                return ServiceResult<RollDiceResult>.Fail(
                    new ServiceError(ErrorType.Validation, "Player is in jail. Use jail-specific actions.")
                );

            // Validasi: cek apakah pemain sudah roll di turn ini
            if (_hasRolledThisTurn.ContainsKey(CurrentPlayer) && _hasRolledThisTurn[CurrentPlayer])
                return ServiceResult<RollDiceResult>.Fail(
                    new ServiceError(ErrorType.Validation, "You have already rolled this turn.")
                );

            var rollResult = RollDices();
            if (!rollResult.IsSuccess || rollResult.Data == null)
                return ServiceResult<RollDiceResult>.Fail(
                    new ServiceError(ErrorType.Validation, "Failed to roll dice.")
                );

            var roll = rollResult.Data;

            // Move player
            var moveResult = MovePlayer(roll.Total);
            if (!moveResult.IsSuccess)
                return ServiceResult<RollDiceResult>.Fail(moveResult.Error!);

            // Handle landing
            OnLand();

            var result = new RollDiceResult
            {
                Roll = roll,
                Move = new MoveResult
                {
                    NewPosition = CurrentPlayer.PathIndex,
                    TileName = CurrentPlayer.CurrentTile?.Name ?? "",
                    TileType = CurrentPlayer.CurrentTile?.TilesType.ToString() ?? ""
                }
            };

            // Set flag bahwa pemain sudah roll
            _hasRolledThisTurn[CurrentPlayer] = true;

            return ServiceResult<RollDiceResult>.Success(result);
        }

        public ServiceResult<PropertyActionResult> ExecuteBuyProperty(string playerName)
        {
            var validationResult = ValidatePlayerTurn(playerName);
            if (!validationResult.IsSuccess)
                return ServiceResult<PropertyActionResult>.Fail(validationResult.Error!);

            var tile = CurrentPlayer.CurrentTile;
            if (tile == null)
                return ServiceResult<PropertyActionResult>.Fail(
                    new ServiceError(ErrorType.Validation, "Player is not on a valid tile.")
                );

            if (!TileAssets.ContainsKey(tile) || TileAssets[tile] == null)
                return ServiceResult<PropertyActionResult>.Fail(
                    new ServiceError(ErrorType.Validation, "This tile has no property to buy.")
                );

            var asset = TileAssets[tile]!;
            var buyResult = PlayerBuyAsset(asset);
            
            var result = new PropertyActionResult
            {
                Success = buyResult.IsSuccess,
                Message = buyResult.IsSuccess ? $"Successfully bought {asset.Name}" : buyResult.Error?.Message ?? "Failed to buy property"
            };

            return buyResult.IsSuccess 
                ? ServiceResult<PropertyActionResult>.Success(result)
                : ServiceResult<PropertyActionResult>.Fail(buyResult.Error!);
        }

        public ServiceResult<PropertyActionResult> ExecuteBuildHouse(string playerName, string propertyName)
        {
            var validationResult = ValidatePlayerTurn(playerName);
            if (!validationResult.IsSuccess)
                return ServiceResult<PropertyActionResult>.Fail(validationResult.Error!);

            var asset = CurrentPlayer.Assets.FirstOrDefault(a => a.Name == propertyName);
            if (asset == null)
                return ServiceResult<PropertyActionResult>.Fail(
                    new ServiceError(ErrorType.Validation, $"Property '{propertyName}' not found in player's assets.")
                );

            var buildResult = PlayerAddHouse(asset);
            
            var result = new PropertyActionResult
            {
                Success = buildResult.IsSuccess,
                Message = buildResult.IsSuccess ? $"Built house on {propertyName}" : buildResult.Error?.Message ?? "Failed to build house"
            };

            return buildResult.IsSuccess 
                ? ServiceResult<PropertyActionResult>.Success(result)
                : ServiceResult<PropertyActionResult>.Fail(buildResult.Error!);
        }

        public ServiceResult<PropertyActionResult> ExecuteSellHouse(string playerName, string propertyName)
        {
            var validationResult = ValidatePlayerTurn(playerName);
            if (!validationResult.IsSuccess)
                return ServiceResult<PropertyActionResult>.Fail(validationResult.Error!);

            var asset = CurrentPlayer.Assets.FirstOrDefault(a => a.Name == propertyName);
            if (asset == null)
                return ServiceResult<PropertyActionResult>.Fail(
                    new ServiceError(ErrorType.Validation, $"Property '{propertyName}' not found in player's assets.")
                );

            var sellResult = PlayerSellHouse(asset);
            
            var result = new PropertyActionResult
            {
                Success = sellResult.IsSuccess,
                Message = sellResult.IsSuccess ? $"Sold house on {propertyName}" : sellResult.Error?.Message ?? "Failed to sell house"
            };

            return sellResult.IsSuccess 
                ? ServiceResult<PropertyActionResult>.Success(result)
                : ServiceResult<PropertyActionResult>.Fail(sellResult.Error!);
        }

        public ServiceResult<PropertyActionResult> ExecuteMortgage(string playerName, string propertyName)
        {
            var validationResult = ValidatePlayerTurn(playerName);
            if (!validationResult.IsSuccess)
                return ServiceResult<PropertyActionResult>.Fail(validationResult.Error!);

            var asset = CurrentPlayer.Assets.FirstOrDefault(a => a.Name == propertyName);
            if (asset == null)
                return ServiceResult<PropertyActionResult>.Fail(
                    new ServiceError(ErrorType.Validation, $"Property '{propertyName}' not found in player's assets.")
                );

            var mortgageResult = PlayerMortgageAsset(CurrentPlayer, asset);
            
            var result = new PropertyActionResult
            {
                Success = mortgageResult.IsSuccess,
                Message = mortgageResult.IsSuccess ? $"Mortgaged {propertyName}" : mortgageResult.Error?.Message ?? "Failed to mortgage"
            };

            return mortgageResult.IsSuccess 
                ? ServiceResult<PropertyActionResult>.Success(result)
                : ServiceResult<PropertyActionResult>.Fail(mortgageResult.Error!);
        }

        public ServiceResult<PropertyActionResult> ExecuteUnmortgage(string playerName, string propertyName)
        {
            var validationResult = ValidatePlayerTurn(playerName);
            if (!validationResult.IsSuccess)
                return ServiceResult<PropertyActionResult>.Fail(validationResult.Error!);

            var asset = CurrentPlayer.Assets.FirstOrDefault(a => a.Name == propertyName);
            if (asset == null)
                return ServiceResult<PropertyActionResult>.Fail(
                    new ServiceError(ErrorType.Validation, $"Property '{propertyName}' not found in player's assets.")
                );

            var unmortgageResult = PlayerUnmortgageAsset(CurrentPlayer, asset);
            
            var result = new PropertyActionResult
            {
                Success = unmortgageResult.IsSuccess,
                Message = unmortgageResult.IsSuccess ? $"Unmortgaged {propertyName}" : unmortgageResult.Error?.Message ?? "Failed to unmortgage"
            };

            return unmortgageResult.IsSuccess 
                ? ServiceResult<PropertyActionResult>.Success(result)
                : ServiceResult<PropertyActionResult>.Fail(unmortgageResult.Error!);
        }

        public ServiceResult<TradeResult> ExecuteTrade(TradeRequest request)
        {
            var player1 = Players.FirstOrDefault(p => p.Name == request.PlayerName);
            var player2 = Players.FirstOrDefault(p => p.Name == request.TargetPlayerName);

            if (player1 == null)
                return ServiceResult<TradeResult>.Fail(
                    new ServiceError(ErrorType.Validation, $"Player '{request.PlayerName}' not found.")
                );

            if (player2 == null)
                return ServiceResult<TradeResult>.Fail(
                    new ServiceError(ErrorType.Validation, $"Target player '{request.TargetPlayerName}' not found.")
                );

            // Find assets by name
            var offer1 = new List<IAsset>();
            foreach (var propName in request.OfferedProperties)
            {
                var asset = player1.Assets.FirstOrDefault(a => a.Name == propName);
                if (asset == null)
                    return ServiceResult<TradeResult>.Fail(
                        new ServiceError(ErrorType.Validation, $"Property '{propName}' not found in {player1.Name}'s assets.")
                    );
                offer1.Add(asset);
            }

            var offer2 = new List<IAsset>();
            foreach (var propName in request.RequestedProperties)
            {
                var asset = player2.Assets.FirstOrDefault(a => a.Name == propName);
                if (asset == null)
                    return ServiceResult<TradeResult>.Fail(
                        new ServiceError(ErrorType.Validation, $"Property '{propName}' not found in {player2.Name}'s assets.")
                    );
                offer2.Add(asset);
            }

            var tradeResult = PlayerProposeTrade(player1, player2, offer1, request.OfferedMoney, offer2, request.RequestedMoney);
            
            var result = new TradeResult
            {
                Success = tradeResult.IsSuccess,
                Message = tradeResult.IsSuccess ? "Trade completed successfully" : tradeResult.Error?.Message ?? "Trade failed",
                Player1Name = player1.Name,
                Player2Name = player2.Name
            };

            return tradeResult.IsSuccess 
                ? ServiceResult<TradeResult>.Success(result)
                : ServiceResult<TradeResult>.Fail(tradeResult.Error!);
        }

        public ServiceResult<bool> ExecutePayJailFee(string playerName)
        {
            var validationResult = ValidatePlayerTurn(playerName);
            if (!validationResult.IsSuccess)
                return ServiceResult<bool>.Fail(validationResult.Error!);

            var payResult = PayJailFee();
            return payResult;
        }

        public ServiceResult<bool> ExecuteUseJailCard(string playerName)
        {
            var validationResult = ValidatePlayerTurn(playerName);
            if (!validationResult.IsSuccess)
                return ServiceResult<bool>.Fail(validationResult.Error!);

            var useCardResult = UseGetOutOfJailCard();
            return useCardResult;
        }

        public ServiceResult<RollDiceResult> ExecuteTryRollDoublesInJail(string playerName)
        {
            var validationResult = ValidatePlayerTurn(playerName);
            if (!validationResult.IsSuccess)
                return ServiceResult<RollDiceResult>.Fail(validationResult.Error!);

            if (CurrentPlayer.PlayerState != PlayerStateEnum.InJail)
                return ServiceResult<RollDiceResult>.Fail(
                    new ServiceError(ErrorType.Validation, "Player is not in jail.")
                );

            var rollResult = RollDices();
            if (!rollResult.IsSuccess || rollResult.Data == null)
                return ServiceResult<RollDiceResult>.Fail(
                    new ServiceError(ErrorType.Validation, "Failed to roll dice.")
                );

            var roll = rollResult.Data;

            // TryRollDoublesInJail handles move if double is rolled
            var doubleResult = TryRollDoublesInJail();

            var result = new RollDiceResult
            {
                Roll = roll,
                Move = new MoveResult
                {
                    NewPosition = CurrentPlayer.PathIndex,
                    TileName = CurrentPlayer.CurrentTile?.Name ?? "Jail",
                    TileType = CurrentPlayer.CurrentTile?.TilesType.ToString() ?? "Special"
                }
            };

            return ServiceResult<RollDiceResult>.Success(result);
        }

        public ServiceResult<bool> ExecuteEndTurn(string playerName)
        {
            var validationResult = ValidatePlayerTurn(playerName);
            if (!validationResult.IsSuccess)
                return ServiceResult<bool>.Fail(validationResult.Error!);

            // Validasi: pemain harus sudah roll dice
            if (!_hasRolledThisTurn.ContainsKey(CurrentPlayer) || !_hasRolledThisTurn[CurrentPlayer])
                return ServiceResult<bool>.Fail(
                    new ServiceError(ErrorType.Validation, "You must roll dice before ending turn.")
                );

            NextTurn();
            return ServiceResult<bool>.Success(true);
        }

        public GameData GetGameState()
        {
            var playerData = Players.Select(MapPlayerToData).ToList();
            var propertyData = TileAssets.Values
                .Where(a => a != null)
                .Select(a => MapPropertyToData(a!))
                .ToList();

            return new GameData
            {
                IsGameOver = IsGameOver,
                WinnerName = Winner?.Name,
                CurrentTurn = CurrentTurn,
                CurrentPlayerName = CurrentPlayer.Name,
                Players = playerData,
                AllProperties = propertyData,
                AvailableActions = GetAvailableActionsForCurrentPlayer()
            };
        }

        private PlayerData MapPlayerToData(IPlayer player)
        {
            var currentTile = player.CurrentTile ?? Board.Path[0];
            var properties = player.Assets.Select(MapPropertyToData).ToList();

            return new PlayerData
            {
                Name = player.Name,
                Money = GetPlayerMoney(player).Data,
                Position = player.PathIndex,
                CurrentTileName = currentTile.Name,
                CurrentTileType = currentTile.TilesType.ToString(),
                State = player.PlayerState.ToString(),
                JailTurns = _playerJailTurns.ContainsKey(player) ? _playerJailTurns[player] : 0,
                HasGetOutOfJailCard = _playerGetOutOfJailCards.ContainsKey(player) && _playerGetOutOfJailCards[player] > 0,
                Properties = properties
            };
        }

        private PropertyData MapPropertyToData(IAsset asset)
        {
            int rent = 0;
            if (asset.Owner != null)
            {
                var rentResult = CalculateRent(asset);
                if (rentResult.IsSuccess)
                {
                    rent = rentResult.Data;
                }
            }

            return new PropertyData
            {
                Name = asset.Name,
                Type = asset.TypeAsset.ToString(),
                Price = asset.Value,
                IsMortgaged = asset.AssetCondition == AssetCondition.Mortgage,
                Houses = asset.AmountHouse
            };
        }

        private List<string> GetAvailableActionsForCurrentPlayer()
        {
            var actions = new List<string>();

            if (IsGameOver)
                return actions;

            var player = CurrentPlayer;

            // Roll dice action (always available unless in jail with other options)
            if (player.PlayerState == PlayerStateEnum.InJail)
            {
                actions.Add("pay-jail-fee");
                actions.Add("try-roll-doubles");
                if (_playerGetOutOfJailCards[player] > 0)
                {
                    actions.Add("use-jail-card");
                }
            }
            else
            {
                actions.Add("roll-dice");
            }

            // Property actions on current tile
            var tile = player.CurrentTile;
            if (tile != null && TileAssets.ContainsKey(tile))
            {
                var asset = TileAssets[tile];
                if (asset != null && asset.Owner == null)
                {
                    // Can buy property
                    if (GetPlayerMoney(player).Data >= asset.Value)
                    {
                        actions.Add("buy-property");
                    }
                }
            }

            // Property management actions
            if (player.Assets.Any(a => a.TypeAsset == TypeAsset.RealEstate && a.AmountHouse < 5 && a.AssetCondition == AssetCondition.Normal))
            {
                actions.Add("build-house");
            }

            if (player.Assets.Any(a => a.AmountHouse > 0))
            {
                actions.Add("sell-house");
            }

            if (player.Assets.Any(a => a.AssetCondition == AssetCondition.Normal && a.AmountHouse == 0))
            {
                actions.Add("mortgage-property");
            }

            if (player.Assets.Any(a => a.AssetCondition == AssetCondition.Mortgage))
            {
                actions.Add("unmortgage-property");
            }

            // Trade action (always available if player has assets or opponents exist)
            if (GetActivePlayers().Count > 1)
            {
                actions.Add("trade");
            }

            actions.Add("end-turn");

            return actions;
        }

        // ========== FORCE END GAME METHODS ==========

        /// <summary>
        /// Menghitung total kekayaan pemain (uang + nilai aset)
        /// </summary>
        public ServiceResult<int> CalculatePlayerTotalWealth(IPlayer player)
        {
            // Uang cash
            var moneyResult = GetPlayerMoney(player);
            if (!moneyResult.IsSuccess)
                return ServiceResult<int>.Fail(moneyResult.Error!);
            
            // Nilai aset (properti + rumah)
            var assetsResult = CalculatePlayerTotalAssetsValue(player);
            if (!assetsResult.IsSuccess)
                return ServiceResult<int>.Fail(assetsResult.Error!);
            
            int totalWealth = moneyResult.Data + assetsResult.Data;
            return ServiceResult<int>.Success(totalWealth);
        }

        /// <summary>
        /// Force end game dan tentukan pemenang berdasarkan total kekayaan
        /// </summary>
        public ServiceResult<ForceEndGameResult> ExecuteForceEndGame()
        {
            if (IsGameOver)
                return ServiceResult<ForceEndGameResult>.Fail(
                    new ServiceError(ErrorType.Validation, "Game is already over.")
                );

            // Hitung kekayaan semua pemain (termasuk bankrupt)
            var rankings = new List<PlayerRanking>();
            
            foreach (var player in Players)
            {
                var wealthResult = CalculatePlayerTotalWealth(player);
                var moneyResult = GetPlayerMoney(player);
                var assetsResult = CalculatePlayerTotalAssetsValue(player);
                
                rankings.Add(new PlayerRanking
                {
                    PlayerName = player.Name,
                    TotalWealth = wealthResult.Data,
                    Cash = moneyResult.Data,
                    AssetsValue = assetsResult.Data,
                    PropertyCount = player.Assets.Count,
                    HouseCount = player.Assets.Sum(a => a.AmountHouse)
                });
            }
            
            // Sort berdasarkan total wealth (descending)
            rankings = rankings.OrderByDescending(r => r.TotalWealth).ToList();
            
            // Assign rank
            for (int i = 0; i < rankings.Count; i++)
            {
                rankings[i].Rank = i + 1;
            }
            
            // Set winner (pemain dengan total wealth terbanyak)
            var winner = Players.First(p => p.Name == rankings[0].PlayerName);
            IsGameOver = true;
            Winner = winner;
            
            var result = new ForceEndGameResult
            {
                IsGameOver = true,
                WinnerName = winner.Name,
                TotalTurns = CurrentTurn,
                Rankings = rankings
            };
            
            return ServiceResult<ForceEndGameResult>.Success(result);
        }

    }
}