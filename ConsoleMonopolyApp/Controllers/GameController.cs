using ConsoleMonopolyApp.Enums;
using ConsoleMonopolyApp.Interfaces;
using ConsoleMonopolyApp.Models;

namespace ConsoleMonopolyApp.Controllers;

public class GameController
{
    public IBoard Board { get; }
    public List<IPlayer> Players { get; }
    public List<IDice> Dices { get; }
    public Dictionary<IPlayer, List<IAsset>> PlayerAssets { get; }
    public Dictionary<ITile, IAsset?> TileAssets { get; }
    public IDecks CommunityChestDeck { get; }
    public IDecks ChanceDeck { get; }
    public int CurrentTurn { get; private set; }
    public IPlayer CurrentPlayer => Players[CurrentTurn % Players.Count];
    public bool IsGameOver { get; private set; }
    public IPlayer? Winner { get; private set; }
    public int LastDiceRoll { get; private set; }

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

    public GameController(IBoard board, List<IPlayer> players, List<IDice> dices, 
                          IDecks communityChestDeck, IDecks chanceDeck)
    {
        if (players.Count < 2 || players.Count > 4)
            throw new ArgumentException("Game requires 2-4 players");

        Board = board;
        Players = players;
        Dices = dices;
        CommunityChestDeck = communityChestDeck;
        ChanceDeck = chanceDeck;
        CurrentTurn = 0;
        IsGameOver = false;
        Winner = null;
        LastDiceRoll = 0;

        PlayerAssets = new Dictionary<IPlayer, List<IAsset>>();
        TileAssets = new Dictionary<ITile, IAsset?>();

        foreach (var player in players)
        {
            PlayerAssets[player] = new List<IAsset>();
            player.RouteIndex = 0;
            player.CurrentTile = Board.Route[0];
        }

        foreach (var tile in Board.Route)
        {
            TileAssets[tile] = tile.Asset;
        }
    }

    public void StartGame()
    {
        OnMessage?.Invoke("Game Started! Welcome to Monopoly!");
        OnMessage?.Invoke($"Players: {string.Join(", ", Players.Select(p => p.Name))}");
        OnMessage?.Invoke($"{CurrentPlayer.Name}'s turn!");
    }

    public void NextTurn()
    {
        // Skip bankrupt players
        do
        {
            CurrentTurn++;
        } while (CurrentPlayer.State == PlayerState.Bankrupt && GetActivePlayers().Count > 1);

        var activePlayers = GetActivePlayers();
        if (activePlayers.Count == 1)
        {
            IsGameOver = true;
            Winner = activePlayers[0];
            OnPlayerWins?.Invoke(Winner);
            return;
        }

        OnMessage?.Invoke($"\n--- {CurrentPlayer.Name}'s turn ---");
    }

    public (int dice1, int dice2) RollDice()
    {
        if (Dices.Count < 2)
            throw new InvalidOperationException("Need at least 2 dice");

        int dice1 = Dices[0].Roll();
        int dice2 = Dices[1].Roll();
        LastDiceRoll = dice1 + dice2;

        OnDiceRolled?.Invoke(CurrentPlayer, dice1, dice2);

        return (dice1, dice2);
    }

    public int Roll()
    {
        int total = 0;
        foreach (var dice in Dices)
        {
            total += dice.Roll();
        }
        LastDiceRoll = total;
        return total;
    }

    public bool HandleJailTurn()
    {
        if (CurrentPlayer.State != PlayerState.InJail)
            return false;

        CurrentPlayer.JailTurns++;

        // Options: Pay $50, Use Get Out of Jail card, or try to roll doubles
        return true;
    }

    public bool PayJailFee()
    {
        if (CurrentPlayer.State != PlayerState.InJail)
            return false;

        if (CurrentPlayer.Money.Subtract(JAIL_FEE))
        {
            CurrentPlayer.State = PlayerState.Normal;
            CurrentPlayer.JailTurns = 0;
            OnMessage?.Invoke($"{CurrentPlayer.Name} paid ${JAIL_FEE} to get out of jail.");
            return true;
        }

        OnMessage?.Invoke($"{CurrentPlayer.Name} doesn't have enough money to pay jail fee.");
        return false;
    }

    public bool UseGetOutOfJailCard()
    {
        if (CurrentPlayer.State != PlayerState.InJail || !CurrentPlayer.HasGetOutOfJailCard)
            return false;

        CurrentPlayer.HasGetOutOfJailCard = false;
        CurrentPlayer.State = PlayerState.Normal;
        CurrentPlayer.JailTurns = 0;
        OnMessage?.Invoke($"{CurrentPlayer.Name} used Get Out of Jail Free card!");
        return true;
    }

    public bool TryRollDoublesForJail()
    {
        if (CurrentPlayer.State != PlayerState.InJail)
            return false;

        var (dice1, dice2) = RollDice();

        if (dice1 == dice2)
        {
            CurrentPlayer.State = PlayerState.Normal;
            CurrentPlayer.JailTurns = 0;
            OnMessage?.Invoke($"{CurrentPlayer.Name} rolled doubles ({dice1}, {dice2}) and is out of jail!");
            return true;
        }

        if (CurrentPlayer.JailTurns >= 3)
        {
            // Must pay after 3 turns
            if (!CurrentPlayer.Money.Subtract(JAIL_FEE))
            {
                CheckIsBankrupt(CurrentPlayer);
            }
            CurrentPlayer.State = PlayerState.Normal;
            CurrentPlayer.JailTurns = 0;
            OnMessage?.Invoke($"{CurrentPlayer.Name} must pay ${JAIL_FEE} after 3 turns in jail.");
        }
        else
        {
            OnMessage?.Invoke($"{CurrentPlayer.Name} didn't roll doubles. Still in jail.");
        }

        return false;
    }

    public ITile MovePlayer(int steps)
    {
        int oldPosition = CurrentPlayer.RouteIndex;
        int newPosition = (oldPosition + steps) % Board.Route.Count;

        // Check if passed GO
        if (newPosition < oldPosition && steps > 0)
        {
            CurrentPlayer.Money.Add(GO_SALARY);
            OnMessage?.Invoke($"{CurrentPlayer.Name} passed GO and collected ${GO_SALARY}!");
        }

        CurrentPlayer.RouteIndex = newPosition;
        CurrentPlayer.CurrentTile = Board.Route[newPosition];

        OnPlayerMoved?.Invoke(CurrentPlayer, CurrentPlayer.CurrentTile);
        OnMessage?.Invoke($"{CurrentPlayer.Name} landed on {CurrentPlayer.CurrentTile.Name}");

        return CurrentPlayer.CurrentTile;
    }

    public void MovePlayerToPosition(int position)
    {
        int oldPosition = CurrentPlayer.RouteIndex;

        // Check if passed GO
        if (position < oldPosition)
        {
            CurrentPlayer.Money.Add(GO_SALARY);
            OnMessage?.Invoke($"{CurrentPlayer.Name} passed GO and collected ${GO_SALARY}!");
        }

        CurrentPlayer.RouteIndex = position;
        CurrentPlayer.CurrentTile = Board.Route[position];
        OnPlayerMoved?.Invoke(CurrentPlayer, CurrentPlayer.CurrentTile);
    }

    public void SendToJail()
    {
        CurrentPlayer.RouteIndex = JAIL_POSITION;
        CurrentPlayer.CurrentTile = Board.Route[JAIL_POSITION];
        CurrentPlayer.State = PlayerState.InJail;
        CurrentPlayer.JailTurns = 0;
        OnMessage?.Invoke($"{CurrentPlayer.Name} was sent to Jail!");
    }

    public void OnLand()
    {
        var tile = CurrentPlayer.CurrentTile;
        if (tile == null) return;

        switch (tile.EffectType)
        {
            case EffectType.GO:
                // Already handled in MovePlayer if passing GO
                break;

            case EffectType.COMMUNITY_CHEST:
                GetAndApplyDeck(CommunityChestDeck);
                break;

            case EffectType.CHANCE:
                GetAndApplyDeck(ChanceDeck);
                break;

            case EffectType.TAX:
                int taxAmount = tile.Name.Contains("Luxury") ? LUXURY_TAX : TAX_AMOUNT;
                if (!CurrentPlayer.Money.Subtract(taxAmount))
                {
                    CheckIsBankrupt(CurrentPlayer);
                }
                OnMessage?.Invoke($"{CurrentPlayer.Name} paid ${taxAmount} in taxes.");
                break;

            case EffectType.GO_TO_JAIL:
                SendToJail();
                break;

            case EffectType.JAIL:
                // Just visiting
                OnMessage?.Invoke($"{CurrentPlayer.Name} is just visiting Jail.");
                break;

            case EffectType.FREE_PARKING:
                OnMessage?.Invoke($"{CurrentPlayer.Name} is relaxing at Free Parking.");
                break;

            case EffectType.NOTHING:
                // Property tiles
                if (tile.Asset != null)
                {
                    HandlePropertyTile(tile);
                }
                break;
        }
    }

    private void HandlePropertyTile(ITile tile)
    {
        var asset = tile.Asset!;

        if (asset.Owner == null)
        {
            // Property available for purchase
            OnMessage?.Invoke($"{tile.Name} is available for ${asset.Value}");
        }
        else if (asset.Owner != CurrentPlayer)
        {
            // Pay rent
            if (asset.AssetsCondition != AssetsCondition.MORTGAGED)
            {
                int sameTypeCount = CountSameTypeAssets(asset.Owner, asset);
                int rent = asset.CalculateRent(LastDiceRoll, sameTypeCount);
                
                if (CurrentPlayer.Money.Subtract(rent))
                {
                    asset.Owner.Money.Add(rent);
                    OnRentPaid?.Invoke(CurrentPlayer, rent);
                    OnMessage?.Invoke($"{CurrentPlayer.Name} paid ${rent} rent to {asset.Owner.Name}");
                }
                else
                {
                    OnMessage?.Invoke($"{CurrentPlayer.Name} cannot afford ${rent} rent!");
                    CheckIsBankrupt(CurrentPlayer);
                }
            }
            else
            {
                OnMessage?.Invoke($"{tile.Name} is mortgaged. No rent to pay.");
            }
        }
        else
        {
            OnMessage?.Invoke($"{CurrentPlayer.Name} owns this property.");
        }
    }

    private int CountSameTypeAssets(IPlayer owner, IAsset asset)
    {
        int count = 0;
        foreach (var a in owner.Assets)
        {
            if (asset.TypeAsset == TypeAsset.REAL_ESTATE)
            {
                if (a.ColorGroup == asset.ColorGroup)
                    count++;
            }
            else if (a.TypeAsset == asset.TypeAsset)
            {
                count++;
            }
        }
        return count;
    }

    public bool PlayerBuyAsset(IAsset asset)
    {
        if (asset.Owner != null)
        {
            OnMessage?.Invoke("This property is already owned.");
            return false;
        }

        if (!CurrentPlayer.Money.Subtract(asset.Value))
        {
            OnMessage?.Invoke($"{CurrentPlayer.Name} doesn't have enough money to buy {asset.Name}.");
            return false;
        }

        CurrentPlayer.AddAsset(asset);
        PlayerAssets[CurrentPlayer].Add(asset);
        OnPropertyBought?.Invoke(CurrentPlayer, asset);
        OnMessage?.Invoke($"{CurrentPlayer.Name} bought {asset.Name} for ${asset.Value}!");
        return true;
    }

    public bool PlayerMortgageAsset(IPlayer player, IAsset asset)
    {
        if (asset.Owner != player)
        {
            OnMessage?.Invoke("Player doesn't own this property.");
            return false;
        }

        if (asset.AssetsCondition == AssetsCondition.MORTGAGED)
        {
            OnMessage?.Invoke("Property is already mortgaged.");
            return false;
        }

        if (asset.AmountHouse > 0)
        {
            OnMessage?.Invoke("Must sell all houses before mortgaging.");
            return false;
        }

        asset.AssetsCondition = AssetsCondition.MORTGAGED;
        int mortgageValue = asset.GetMortgageValue();
        player.Money.Add(mortgageValue);
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

        if (asset.AssetsCondition != AssetsCondition.MORTGAGED)
        {
            OnMessage?.Invoke("Property is not mortgaged.");
            return false;
        }

        int unmortgageValue = asset.GetUnmortgageValue();
        if (!player.Money.Subtract(unmortgageValue))
        {
            OnMessage?.Invoke($"Not enough money to unmortgage. Need ${unmortgageValue}.");
            return false;
        }

        asset.AssetsCondition = AssetsCondition.NORMAL;
        OnMessage?.Invoke($"{player.Name} unmortgaged {asset.Name} for ${unmortgageValue}.");
        return true;
    }

    public bool PlayerAddHouse(IAsset asset)
    {
        if (asset.Owner != CurrentPlayer)
        {
            OnMessage?.Invoke("You don't own this property.");
            return false;
        }

        if (asset.TypeAsset != TypeAsset.REAL_ESTATE)
        {
            OnMessage?.Invoke("Can only build houses on real estate.");
            return false;
        }

        if (asset.AmountHouse >= 5)
        {
            OnMessage?.Invoke("Maximum houses (hotel) already built.");
            return false;
        }

        // Check if player owns all properties in color group
        if (!OwnsFullColorGroup(CurrentPlayer, asset.ColorGroup))
        {
            OnMessage?.Invoke("Must own all properties in this color group to build.");
            return false;
        }

        if (!CurrentPlayer.Money.Subtract(asset.HouseCost))
        {
            OnMessage?.Invoke($"Not enough money. House costs ${asset.HouseCost}.");
            return false;
        }

        asset.AmountHouse++;
        string buildingType = asset.AmountHouse == 5 ? "hotel" : "house";
        OnMessage?.Invoke($"{CurrentPlayer.Name} built a {buildingType} on {asset.Name}.");
        return true;
    }

    public bool PlayerSellHouse(IAsset asset)
    {
        if (asset.Owner != CurrentPlayer)
        {
            OnMessage?.Invoke("You don't own this property.");
            return false;
        }

        if (asset.AmountHouse <= 0)
        {
            OnMessage?.Invoke("No houses to sell.");
            return false;
        }

        asset.AmountHouse--;
        int sellPrice = asset.HouseCost / 2;
        CurrentPlayer.Money.Add(sellPrice);
        OnMessage?.Invoke($"{CurrentPlayer.Name} sold a house on {asset.Name} for ${sellPrice}.");
        return true;
    }

    private bool OwnsFullColorGroup(IPlayer player, int colorGroup)
    {
        int required = (colorGroup == 1 || colorGroup == 8) ? 2 : 3;
        int count = player.Assets.Count(a => a.ColorGroup == colorGroup);
        return count >= required;
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
                OnMessage?.Invoke($"{player1.Name} doesn't own {asset.Name}.");
                return false;
            }
        }

        foreach (var asset in offer2)
        {
            if (asset.Owner != player2)
            {
                OnMessage?.Invoke($"{player2.Name} doesn't own {asset.Name}.");
                return false;
            }
        }

        // Check money
        if (player1.Money.Balance < money1)
        {
            OnMessage?.Invoke($"{player1.Name} doesn't have ${money1}.");
            return false;
        }

        if (player2.Money.Balance < money2)
        {
            OnMessage?.Invoke($"{player2.Name} doesn't have ${money2}.");
            return false;
        }

        // Execute trade
        foreach (var asset in offer1)
        {
            player1.RemoveAsset(asset);
            PlayerAssets[player1].Remove(asset);
            player2.AddAsset(asset);
            PlayerAssets[player2].Add(asset);
        }

        foreach (var asset in offer2)
        {
            player2.RemoveAsset(asset);
            PlayerAssets[player2].Remove(asset);
            player1.AddAsset(asset);
            PlayerAssets[player1].Add(asset);
        }

        if (money1 > 0)
        {
            player1.Money.Subtract(money1);
            player2.Money.Add(money1);
        }

        if (money2 > 0)
        {
            player2.Money.Subtract(money2);
            player1.Money.Add(money2);
        }

        OnMessage?.Invoke($"Trade completed between {player1.Name} and {player2.Name}!");
        return true;
    }

    public void GetAndApplyDeck(IDecks deck)
    {
        var card = deck.DrawCard();
        OnCardDrawn?.Invoke(card);
        OnMessage?.Invoke($"Card: {card.Name} - {card.Description}");

        switch (card.CardEffect)
        {
            case CardEffect.RECEIVE_MONEY:
                CurrentPlayer.Money.Add(card.Value);
                OnMessage?.Invoke($"{CurrentPlayer.Name} received ${card.Value}.");
                break;

            case CardEffect.PAY_MONEY:
                if (!CurrentPlayer.Money.Subtract(card.Value))
                {
                    CheckIsBankrupt(CurrentPlayer);
                }
                else
                {
                    OnMessage?.Invoke($"{CurrentPlayer.Name} paid ${card.Value}.");
                }
                break;

            case CardEffect.GO_TO_JAIL:
                SendToJail();
                break;

            case CardEffect.GET_OUT_OF_JAIL:
                CurrentPlayer.HasGetOutOfJailCard = true;
                OnMessage?.Invoke($"{CurrentPlayer.Name} received a Get Out of Jail Free card!");
                break;

            case CardEffect.MOVE:
                if (card.Value < 0)
                {
                    // Move backwards
                    MovePlayer(card.Value);
                }
                else
                {
                    // Move to specific position
                    MovePlayerToPosition(card.Value);
                }
                OnLand();
                break;
        }
    }

    public bool CheckIsBankrupt(IPlayer player)
    {
        int totalValue = CalculatePlayerTotalAssetsValue(player);

        if (player.Money.Balance + totalValue < 0)
        {
            player.State = PlayerState.Bankrupt;
            OnPlayerBankrupt?.Invoke(player);
            OnMessage?.Invoke($"{player.Name} is BANKRUPT!");

            // Return assets to bank
            foreach (var asset in player.Assets.ToList())
            {
                asset.Owner = null;
                asset.AmountHouse = 0;
                asset.AssetsCondition = AssetsCondition.NORMAL;
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
            if (asset.AssetsCondition == AssetsCondition.MORTGAGED)
            {
                total += asset.GetMortgageValue();
            }
            else
            {
                total += asset.Value;
            }
            total += asset.AmountHouse * (asset.HouseCost / 2);
        }
        return total;
    }

    public List<IPlayer> GetActivePlayers()
    {
        return Players.Where(p => p.State != PlayerState.Bankrupt).ToList();
    }

    public bool CanBuyCurrentProperty()
    {
        var tile = CurrentPlayer.CurrentTile;
        return tile?.Asset != null && 
               tile.Asset.Owner == null && 
               CurrentPlayer.Money.Balance >= tile.Asset.Value;
    }

    public IAsset? GetCurrentTileAsset()
    {
        return CurrentPlayer.CurrentTile?.Asset;
    }
}
