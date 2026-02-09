namespace MonopolyBackend.Services
{
    using MonopolyBackend.Enums;
    using MonopolyBackend.Interfaces;
    using MonopolyBackend.Models;
    using MonopolyBackend.Models.Results;
    using MonopolyBackend.Common;
    using MonopolyBackend.Structs;
    using System.Collections.Generic;
    using MonopolyBackend.DTOs.Requests;
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
            IPlayer? player = Players.FirstOrDefault(p => p.Name == playerName);
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

            foreach (IPlayer player in Players)
            {
                PlayerAssets[player] = new List<IAsset>();
                PlayerMoney[player] = new List<IMoney> { new Money(1500) };
                _playerJailTurns[player] = 0;
                _playerGetOutOfJailCards[player] = 0;
                _hasRolledThisTurn[player] = false;
            }
        }

        public void NextTurn()
        {
            do
            {
                CurrentTurn++;
            } while (CurrentPlayer.PlayerState == PlayerStateEnum.Bankrupt && GetActivePlayers().Count > 1);

            List<IPlayer> activePlayers = GetActivePlayers();
            if (activePlayers.Count == 1)
            {
                IsGameOver = true;
                Winner = activePlayers[0];
                return;
            }

            _hasRolledThisTurn[CurrentPlayer] = false;
        }

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
            ServiceResult<ICard> cardResult = DrawCardFromDeck(deck);
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
                    ServiceResult<bool> subtractResult = SubtractMoney(CurrentPlayer, card.Value);
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

            if (jailTurns >= 3)
            {
                return PayJailFee();
            }

            return ServiceResult<bool>.Success(true);
        }

        public ServiceResult<bool> PayJailFee()
        {
            if (CurrentPlayer.PlayerState != PlayerStateEnum.InJail)
                return ServiceResult<bool>.Fail(
                    new ServiceError(ErrorType.Validation, "Player is not in jail.")
                );

            ServiceResult<bool> resultSubtract = SubtractMoney(CurrentPlayer, JAIL_FEE);
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

            DiceRoll result = new DiceRoll
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

            ServiceResult<DiceRoll> rollResult = RollDices();
            if (!rollResult.IsSuccess || rollResult.Data == null)
                return ServiceResult<bool>.Fail(
                    new ServiceError(ErrorType.Validation, "Failed to roll dice.")
                );

            DiceRoll diceData = rollResult.Data;

            if (diceData.IsDouble)
            {
                CurrentPlayer.PlayerState = PlayerStateEnum.Normal;
                _playerJailTurns[CurrentPlayer] = 0;

                MovePlayer(diceData.Total);
                OnLand();
                return ServiceResult<bool>.Success(true);
            }
            else
            {
                return ServiceResult<bool>.Success(false);
            }
        }

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

            asset.Owner = CurrentPlayer;
            CurrentPlayer.Assets.Add(asset);
            PlayerAssets[CurrentPlayer].Add(asset);

            return ServiceResult<bool>.Success(true);
        }

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

        public ServiceResult<bool> PlayerProposeTrade(
    IPlayer initiatingPlayer,
    IPlayer targetPlayer,
    List<IAsset> assetsOfferedByInitiator,
    int moneyOfferedByInitiator,
    List<IAsset> assetsRequestedFromTarget,
    int moneyRequestedFromTarget)
        {
            foreach (IAsset asset in assetsOfferedByInitiator)
            {
                if (asset.Owner != initiatingPlayer)
                {
                    return ServiceResult<bool>.Fail(
                        new ServiceError(ErrorType.Validation, $"{initiatingPlayer.Name} tidak memiliki {asset.Name}.")
                    );
                }
            }

            foreach (IAsset asset in assetsRequestedFromTarget)
            {
                if (asset.Owner != targetPlayer)
                {
                    return ServiceResult<bool>.Fail(
                        new ServiceError(ErrorType.Validation, $"{targetPlayer.Name} tidak memiliki {asset.Name}.")
                    );
                }
            }

            ServiceResult<int> initiatingPlayerMoneyResult = GetPlayerMoney(initiatingPlayer);
            ServiceResult<int> targetPlayerMoneyResult = GetPlayerMoney(targetPlayer);

            if (!initiatingPlayerMoneyResult.IsSuccess)
                return ServiceResult<bool>.Fail(initiatingPlayerMoneyResult.Error!);

            if (!targetPlayerMoneyResult.IsSuccess)
                return ServiceResult<bool>.Fail(targetPlayerMoneyResult.Error!);

            if (initiatingPlayerMoneyResult.Data < moneyOfferedByInitiator)
            {
                return ServiceResult<bool>.Fail(
                    new ServiceError(ErrorType.Validation, $"{initiatingPlayer.Name} tidak punya ${moneyOfferedByInitiator}.")
                );
            }

            if (targetPlayerMoneyResult.Data < moneyRequestedFromTarget)
            {
                return ServiceResult<bool>.Fail(
                    new ServiceError(ErrorType.Validation, $"{targetPlayer.Name} tidak punya ${moneyRequestedFromTarget}.")
                );
            }

            foreach (IAsset asset in assetsOfferedByInitiator)
            {
                asset.Owner = targetPlayer;
                initiatingPlayer.Assets.Remove(asset);
                PlayerAssets[initiatingPlayer].Remove(asset);
                targetPlayer.Assets.Add(asset);
                PlayerAssets[targetPlayer].Add(asset);
            }

            foreach (IAsset asset in assetsRequestedFromTarget)
            {
                asset.Owner = initiatingPlayer;
                targetPlayer.Assets.Remove(asset);
                PlayerAssets[targetPlayer].Remove(asset);
                initiatingPlayer.Assets.Add(asset);
                PlayerAssets[initiatingPlayer].Add(asset);
            }

            if (moneyOfferedByInitiator > 0)
            {
                SubtractMoney(initiatingPlayer, moneyOfferedByInitiator);
                AddMoney(targetPlayer, moneyOfferedByInitiator);
            }

            if (moneyRequestedFromTarget > 0)
            {
                SubtractMoney(targetPlayer, moneyRequestedFromTarget);
                AddMoney(initiatingPlayer, moneyRequestedFromTarget);
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

            int sellPrice = asset.Value / 4;
            asset.AmountHouse--;
            AddMoney(CurrentPlayer, sellPrice);

            string buildingType = asset.AmountHouse == 4 ? "hotel" : "rumah";
            return ServiceResult<bool>.Success(true);
        }

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
            ITile? tile = CurrentPlayer.CurrentTile;
            if (tile == null) return;

            switch (tile.EffectType)
            {
                case EffectType.Go:
                    break;

                case EffectType.CommunityChest:
                    GetAndApplyDeck(CommunityChestDeck);
                    break;

                case EffectType.Chance:
                    GetAndApplyDeck(ChanceDeck);
                    break;

                case EffectType.Tax:
                    int taxAmount = tile.Name.Contains("Mewah") ? LUXURY_TAX : TAX_AMOUNT;
                    ServiceResult<bool> subtractTaxResult = SubtractMoney(CurrentPlayer, taxAmount);
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
                    IAsset? asset = TileAssets.ContainsKey(tile) ? TileAssets[tile] : null;
                    if (asset != null || tile.TilesType == TilesType.Property || tile.TilesType == TilesType.Railroad || tile.TilesType == TilesType.Utility)
                    {
                        HandlePropertyTile(tile);
                    }
                    break;
            }
        }

        private void HandlePropertyTile(ITile tile)
        {
            if (!TileAssets.ContainsKey(tile) || TileAssets[tile] == null)
            {
                return;
            }

            IAsset asset = TileAssets[tile]!;

            if (asset.Owner == null)
            {
                return;
            }

            if (asset.Owner == CurrentPlayer)
            {
                return;
            }

            if (asset.AssetCondition == AssetCondition.Mortgage)
            {
                return;
            }

            int rent = CalculateRent(asset).Data;
            ServiceResult<bool> subtractResult = SubtractMoney(CurrentPlayer, rent);

            if (subtractResult.IsSuccess)
            {
                AddMoney(asset.Owner, rent);
            }
            else
            {
                CheckIsBankrupt(CurrentPlayer);
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

            if (asset.TypeAsset == TypeAsset.PublicService)
            {
                int result = sameTypeCount == 1 ? 25 : 50;
                return ServiceResult<int>.Success(result);
            }

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

            int baseRent = asset.Value / 10;

            if (asset.AmountHouse > 0)
            {
                return ServiceResult<int>.Success(asset.AmountHouse switch
                {
                    1 => baseRent * 5,
                    2 => baseRent * 15,
                    3 => baseRent * 45,
                    4 => baseRent * 80,
                    5 => baseRent * 100,
                    _ => baseRent
                });
            }

            return ServiceResult<int>.Success(baseRent);
        }


        public void MovePlayerToPosition(int position)
        {
            int oldPosition = CurrentPlayer.PathIndex;

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

            List<ICard> cards = deck.Cards;
            if (cards == null || cards.Count == 0)
                throw new InvalidOperationException("Deck kosong");

            ICard card = cards[0];
            cards.RemoveAt(0);
            cards.Add(card);

            return ServiceResult<ICard>.Success(card);
        }


        public ServiceResult<bool> CheckIsBankrupt(IPlayer player)
        {
            ServiceResult<int> totalValue = CalculatePlayerTotalAssetsValue(player);
            ServiceResult<int> playerMoney = GetPlayerMoney(player);

            if (playerMoney.Data + totalValue.Data < 0)
            {
                player.PlayerState = PlayerStateEnum.Bankrupt;

                foreach (IAsset asset in player.Assets.ToList())
                {
                    asset.Owner = null;
                    asset.AmountHouse = 0;
                    asset.AssetCondition = AssetCondition.Normal;
                }
                player.Assets.Clear();
                PlayerAssets[player].Clear();

                List<IPlayer> activePlayers = GetActivePlayers();
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
            foreach (IAsset asset in player.Assets)
            {
                if (asset.AssetCondition == AssetCondition.Mortgage)
                {
                    ServiceResult<int> mortgageResult = GetMortgageValue(asset);
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
            int mortageResult = asset.Value / 2;
            return ServiceResult<int>.Success(mortageResult);
        }

        public ServiceResult<int> GetUnmortgageCost(IAsset asset)
        {
            ServiceResult<int> mortgageResult = GetMortgageValue(asset);

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

            IMoney money = new Money(amount);
            PlayerMoney[player].Add(money);
            return ServiceResult<bool>.Success(true);
        }

        public ServiceResult<bool> SubtractMoney(IPlayer player, int amount)
        {
            if (amount <= 0)
                return ServiceResult<bool>.Fail(
                    new ServiceError(ErrorType.Validation, "Amount must be greater than zero.")
                );

            int currentMoney = PlayerMoney[player].Sum(m => m.Balance);
            if (currentMoney < amount)
            {
                return ServiceResult<bool>.Fail(
                    new ServiceError(ErrorType.Validation, "Insufficient funds.")
                );
            }

            int remaining = amount;
            List<IMoney> moneyList = PlayerMoney[player].OrderByDescending(m => m.Balance).ToList();

            foreach (IMoney money in moneyList)
            {
                if (remaining <= 0) break;

                if (money.Balance <= remaining)
                {
                    remaining -= money.Balance;
                    PlayerMoney[player].Remove(money);
                }
                else
                {
                    money.Balance -= remaining;
                    remaining = 0;
                }
            }

            return ServiceResult<bool>.Success(true);
        }

        public ServiceResult<RollDiceResult> ExecuteRollDice(string playerName)
        {
            ServiceResult<IPlayer> validationResult = ValidatePlayerTurn(playerName);
            if (!validationResult.IsSuccess)
                return ServiceResult<RollDiceResult>.Fail(validationResult.Error!);

            if (CurrentPlayer.PlayerState == PlayerStateEnum.InJail)
                return ServiceResult<RollDiceResult>.Fail(
                    new ServiceError(ErrorType.Validation, "Player is in jail. Use jail-specific actions.")
                );

            if (_hasRolledThisTurn.ContainsKey(CurrentPlayer) && _hasRolledThisTurn[CurrentPlayer])
                return ServiceResult<RollDiceResult>.Fail(
                    new ServiceError(ErrorType.Validation, "You have already rolled this turn.")
                );

            ServiceResult<DiceRoll> rollResult = RollDices();
            if (!rollResult.IsSuccess || rollResult.Data == null)
                return ServiceResult<RollDiceResult>.Fail(
                    new ServiceError(ErrorType.Validation, "Failed to roll dice.")
                );

            DiceRoll roll = rollResult.Data;

            ServiceResult<ITile> moveResult = MovePlayer(roll.Total);
            if (!moveResult.IsSuccess)
                return ServiceResult<RollDiceResult>.Fail(moveResult.Error!);

            OnLand();

            RollDiceResult result = new RollDiceResult
            {
                Roll = roll,
                Move = new MoveResult
                {
                    NewPosition = CurrentPlayer.PathIndex,
                    TileName = CurrentPlayer.CurrentTile?.Name ?? "",
                    TileType = CurrentPlayer.CurrentTile?.TilesType.ToString() ?? ""
                }
            };

            _hasRolledThisTurn[CurrentPlayer] = true;

            return ServiceResult<RollDiceResult>.Success(result);
        }

        public ServiceResult<PropertyActionResult> ExecuteBuyProperty(string playerName)
        {
            ServiceResult<IPlayer> validationResult = ValidatePlayerTurn(playerName);
            if (!validationResult.IsSuccess)
                return ServiceResult<PropertyActionResult>.Fail(validationResult.Error!);

            ITile? tile = CurrentPlayer.CurrentTile;
            if (tile == null)
                return ServiceResult<PropertyActionResult>.Fail(
                    new ServiceError(ErrorType.Validation, "Player is not on a valid tile.")
                );

            if (!TileAssets.ContainsKey(tile) || TileAssets[tile] == null)
                return ServiceResult<PropertyActionResult>.Fail(
                    new ServiceError(ErrorType.Validation, "This tile has no property to buy.")
                );

            IAsset asset = TileAssets[tile]!;
            ServiceResult<bool> buyResult = PlayerBuyAsset(asset);

            PropertyActionResult result = new PropertyActionResult
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
            ServiceResult<IPlayer> validationResult = ValidatePlayerTurn(playerName);
            if (!validationResult.IsSuccess)
                return ServiceResult<PropertyActionResult>.Fail(validationResult.Error!);

            IAsset? asset = CurrentPlayer.Assets.FirstOrDefault(a => a.Name == propertyName);
            if (asset == null)
                return ServiceResult<PropertyActionResult>.Fail(
                    new ServiceError(ErrorType.Validation, $"Property '{propertyName}' not found in player's assets.")
                );

            ServiceResult<bool> buildResult = PlayerAddHouse(asset);

            PropertyActionResult result = new PropertyActionResult
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
            ServiceResult<IPlayer> validationResult = ValidatePlayerTurn(playerName);
            if (!validationResult.IsSuccess)
                return ServiceResult<PropertyActionResult>.Fail(validationResult.Error!);

            IAsset? asset = CurrentPlayer.Assets.FirstOrDefault(a => a.Name == propertyName);
            if (asset == null)
                return ServiceResult<PropertyActionResult>.Fail(
                    new ServiceError(ErrorType.Validation, $"Property '{propertyName}' not found in player's assets.")
                );

            ServiceResult<bool> sellResult = PlayerSellHouse(asset);

            PropertyActionResult result = new PropertyActionResult
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
            ServiceResult<IPlayer> validationResult = ValidatePlayerTurn(playerName);
            if (!validationResult.IsSuccess)
                return ServiceResult<PropertyActionResult>.Fail(validationResult.Error!);

            IAsset? asset = CurrentPlayer.Assets.FirstOrDefault(a => a.Name == propertyName);
            if (asset == null)
                return ServiceResult<PropertyActionResult>.Fail(
                    new ServiceError(ErrorType.Validation, $"Property '{propertyName}' not found in player's assets.")
                );

            ServiceResult<bool> mortgageResult = PlayerMortgageAsset(CurrentPlayer, asset);

            PropertyActionResult result = new PropertyActionResult
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
            ServiceResult<IPlayer> validationResult = ValidatePlayerTurn(playerName);
            if (!validationResult.IsSuccess)
                return ServiceResult<PropertyActionResult>.Fail(validationResult.Error!);

            IAsset? asset = CurrentPlayer.Assets.FirstOrDefault(a => a.Name == propertyName);
            if (asset == null)
                return ServiceResult<PropertyActionResult>.Fail(
                    new ServiceError(ErrorType.Validation, $"Property '{propertyName}' not found in player's assets.")
                );

            ServiceResult<bool> unmortgageResult = PlayerUnmortgageAsset(CurrentPlayer, asset);

            PropertyActionResult result = new PropertyActionResult
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
            IPlayer? initiatingPlayer = Players.FirstOrDefault(p => p.Name == request.PlayerName);
            IPlayer? targetPlayer = Players.FirstOrDefault(p => p.Name == request.TargetPlayerName);

            if (initiatingPlayer == null)
                return ServiceResult<TradeResult>.Fail(
                    new ServiceError(ErrorType.Validation, $"Player '{request.PlayerName}' not found.")
                );

            if (targetPlayer == null)
                return ServiceResult<TradeResult>.Fail(
                    new ServiceError(ErrorType.Validation, $"Target player '{request.TargetPlayerName}' not found.")
                );

            List<IAsset> assetsOfferedByInitiator = new List<IAsset>();
            foreach (string propertyName in request.OfferedProperties)
            {
                IAsset? asset = initiatingPlayer.Assets.FirstOrDefault(a => a.Name == propertyName);
                if (asset == null)
                    return ServiceResult<TradeResult>.Fail(
                        new ServiceError(ErrorType.Validation, $"Property '{propertyName}' not found in {initiatingPlayer.Name}'s assets.")
                    );
                assetsOfferedByInitiator.Add(asset);
            }

            List<IAsset> assetsRequestedFromTarget = new List<IAsset>();
            foreach (string propertyName in request.RequestedProperties)
            {
                IAsset? asset = targetPlayer.Assets.FirstOrDefault(a => a.Name == propertyName);
                if (asset == null)
                    return ServiceResult<TradeResult>.Fail(
                        new ServiceError(ErrorType.Validation, $"Property '{propertyName}' not found in {targetPlayer.Name}'s assets.")
                    );
                assetsRequestedFromTarget.Add(asset);
            }

            ServiceResult<bool> tradeResult = PlayerProposeTrade(
                initiatingPlayer: initiatingPlayer,
                targetPlayer: targetPlayer,
                assetsOfferedByInitiator: assetsOfferedByInitiator,
                moneyOfferedByInitiator: request.OfferedMoney,
                assetsRequestedFromTarget: assetsRequestedFromTarget,
                moneyRequestedFromTarget: request.RequestedMoney
            );

            TradeResult result = new TradeResult
            {
                Success = tradeResult.IsSuccess,
                Message = tradeResult.IsSuccess ? "Trade completed successfully" : tradeResult.Error?.Message ?? "Trade failed",
                Player1Name = initiatingPlayer.Name,
                Player2Name = targetPlayer.Name
            };

            return tradeResult.IsSuccess
                ? ServiceResult<TradeResult>.Success(result)
                : ServiceResult<TradeResult>.Fail(tradeResult.Error!);
        }

        public ServiceResult<bool> ExecutePayJailFee(string playerName)
        {
            ServiceResult<IPlayer> validationResult = ValidatePlayerTurn(playerName);
            if (!validationResult.IsSuccess)
                return ServiceResult<bool>.Fail(validationResult.Error!);

            ServiceResult<bool> payResult = PayJailFee();
            return payResult;
        }

        public ServiceResult<bool> ExecuteUseJailCard(string playerName)
        {
            ServiceResult<IPlayer> validationResult = ValidatePlayerTurn(playerName);
            if (!validationResult.IsSuccess)
                return ServiceResult<bool>.Fail(validationResult.Error!);

            ServiceResult<bool> useCardResult = UseGetOutOfJailCard();
            return useCardResult;
        }

        public ServiceResult<RollDiceResult> ExecuteTryRollDoublesInJail(string playerName)
        {
            ServiceResult<IPlayer> validationResult = ValidatePlayerTurn(playerName);
            if (!validationResult.IsSuccess)
                return ServiceResult<RollDiceResult>.Fail(validationResult.Error!);

            if (CurrentPlayer.PlayerState != PlayerStateEnum.InJail)
                return ServiceResult<RollDiceResult>.Fail(
                    new ServiceError(ErrorType.Validation, "Player is not in jail.")
                );

            ServiceResult<DiceRoll> rollResult = RollDices();
            if (!rollResult.IsSuccess || rollResult.Data == null)
                return ServiceResult<RollDiceResult>.Fail(
                    new ServiceError(ErrorType.Validation, "Failed to roll dice.")
                );

            DiceRoll roll = rollResult.Data;

            ServiceResult<bool> doubleResult = TryRollDoublesInJail();

            RollDiceResult result = new RollDiceResult
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
            ServiceResult<IPlayer> validationResult = ValidatePlayerTurn(playerName);
            if (!validationResult.IsSuccess)
                return ServiceResult<bool>.Fail(validationResult.Error!);

            if (!_hasRolledThisTurn.ContainsKey(CurrentPlayer) || !_hasRolledThisTurn[CurrentPlayer])
                return ServiceResult<bool>.Fail(
                    new ServiceError(ErrorType.Validation, "You must roll dice before ending turn.")
                );

            NextTurn();
            return ServiceResult<bool>.Success(true);
        }

        public ServiceResult<GameData> GetGameState()
        {
            List<PlayerData> playerData = Players.Select(player => MapPlayerToData(player).Data!).ToList();
            List<PropertyData> propertyData = TileAssets.Values
                .Where(a => a != null)
                .Select(a => MapPropertyToData(a!).Data!)
                .ToList();

            GameData gameData = new GameData
            {
                IsGameOver = IsGameOver,
                WinnerName = Winner?.Name,
                CurrentTurn = CurrentTurn,
                CurrentPlayerName = CurrentPlayer.Name,
                Players = playerData,
                AllProperties = propertyData,
                AvailableActions = GetAvailableActionsForCurrentPlayer()
            };

            return ServiceResult<GameData>.Success(gameData);
        }

        private ServiceResult<PlayerData> MapPlayerToData(IPlayer player)
        {
            ITile currentTile = player.CurrentTile ?? Board.Path[0];
            List<PropertyData> properties = player.Assets.Select(asset => MapPropertyToData(asset).Data!).ToList();

            ServiceResult<int> moneyResult = GetPlayerMoney(player);
            int playerMoney = moneyResult.IsSuccess ? moneyResult.Data : 0;

            PlayerData resultPlayerData = new PlayerData
            {
                Name = player.Name,
                Money = playerMoney,
                Position = player.PathIndex,
                CurrentTileName = currentTile.Name,
                CurrentTileType = currentTile.TilesType.ToString(),
                State = player.PlayerState.ToString(),
                JailTurns = _playerJailTurns.ContainsKey(player) ? _playerJailTurns[player] : 0,
                HasGetOutOfJailCard = _playerGetOutOfJailCards.ContainsKey(player) && _playerGetOutOfJailCards[player] > 0,
                Properties = properties
            };

            return ServiceResult<PlayerData>.Success(resultPlayerData);
        }

        private ServiceResult<PropertyData> MapPropertyToData(IAsset asset)
        {
            int rent = 0;
            if (asset.Owner != null)
            {
                ServiceResult<int> rentResult = CalculateRent(asset);
                if (rentResult.IsSuccess)
                {
                    rent = rentResult.Data;
                }
            }

            PropertyData resultPropertyData = new PropertyData
            {
                Name = asset.Name,
                Type = asset.TypeAsset.ToString(),
                Price = asset.Value,
                IsMortgaged = asset.AssetCondition == AssetCondition.Mortgage,
                Houses = asset.AmountHouse
            };

            return ServiceResult<PropertyData>.Success(resultPropertyData);
        }

        private List<string> GetAvailableActionsForCurrentPlayer()
        {
            List<string> actions = new List<string>();

            if (IsGameOver)
                return actions;

            IPlayer player = CurrentPlayer;

            if (player.PlayerState == PlayerStateEnum.InJail)
            {
                actions.Add(GameActions.PayJailFee);
                actions.Add(GameActions.TryRollDoubles);
                if (_playerGetOutOfJailCards[player] > 0)
                {
                    actions.Add(GameActions.UseJailCard);
                }
            }
            else
            {
                actions.Add(GameActions.RollDice);
            }

            ITile? tile = player.CurrentTile;
            if (tile != null && TileAssets.ContainsKey(tile))
            {
                IAsset? asset = TileAssets[tile];
                if (asset != null && asset.Owner == null)
                {
                    if (GetPlayerMoney(player).Data >= asset.Value)
                    {
                        actions.Add(GameActions.BuyProperty);
                    }
                }
            }

            if (player.Assets.Any(a => a.TypeAsset == TypeAsset.RealEstate && a.AmountHouse < 5 && a.AssetCondition == AssetCondition.Normal))
            {
                actions.Add(GameActions.BuildHouse);
            }

            if (player.Assets.Any(a => a.AmountHouse > 0))
            {
                actions.Add(GameActions.SellHouse);
            }

            if (player.Assets.Any(a => a.AssetCondition == AssetCondition.Normal && a.AmountHouse == 0))
            {
                actions.Add(GameActions.MortgageProperty);
            }

            if (player.Assets.Any(a => a.AssetCondition == AssetCondition.Mortgage))
            {
                actions.Add(GameActions.UnmortgageProperty);
            }

            if (GetActivePlayers().Count > 1)
            {
                actions.Add(GameActions.Trade);
            }

            actions.Add(GameActions.EndTurn);

            return actions;
        }

        public ServiceResult<int> CalculatePlayerTotalWealth(IPlayer player)
        {
            ServiceResult<int> moneyResult = GetPlayerMoney(player);
            if (!moneyResult.IsSuccess)
                return ServiceResult<int>.Fail(moneyResult.Error!);

            ServiceResult<int> assetsResult = CalculatePlayerTotalAssetsValue(player);
            if (!assetsResult.IsSuccess)
                return ServiceResult<int>.Fail(assetsResult.Error!);

            int totalWealth = moneyResult.Data + assetsResult.Data;
            return ServiceResult<int>.Success(totalWealth);
        }

        public ServiceResult<ForceEndGameResult> ExecuteForceEndGame()
        {
            if (IsGameOver)
                return ServiceResult<ForceEndGameResult>.Fail(
                    new ServiceError(ErrorType.Validation, "Game is already over.")
                );

            List<PlayerRanking> rankings = new List<PlayerRanking>();

            foreach (IPlayer player in Players)
            {
                ServiceResult<int> wealthResult = CalculatePlayerTotalWealth(player);
                ServiceResult<int> moneyResult = GetPlayerMoney(player);
                ServiceResult<int> assetsResult = CalculatePlayerTotalAssetsValue(player);

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

            rankings = rankings.OrderByDescending(r => r.TotalWealth).ToList();

            for (int i = 0; i < rankings.Count; i++)
            {
                rankings[i].Rank = i + 1;
            }

            IPlayer winner = Players.First(p => p.Name == rankings[0].PlayerName);
            IsGameOver = true;
            Winner = winner;

            ForceEndGameResult result = new ForceEndGameResult
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