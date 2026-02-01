using ConsoleMonopolyApp.Controllers;
using ConsoleMonopolyApp.Data;
using ConsoleMonopolyApp.Enums;
using ConsoleMonopolyApp.Interfaces;
using ConsoleMonopolyApp.Models;
using ConsoleMonopolyApp.Views;

namespace ConsoleMonopolyApp;

public class Program
{
    private static GameController? _game;
    private static ConsoleView? _view;

    public static void Main(string[] args)
    {
        _view = new ConsoleView();
        _view.ShowWelcome();

        // Setup players
        var players = SetupPlayers();
        if (players.Count < 2)
        {
            _view.ShowError("Need at least 2 players to play!");
            return;
        }

        // Setup game components
        var board = BoardPreset.CreateStandardBoard();
        var dices = new List<IDice> { new Dice(6), new Dice(6) };
        var communityChestDeck = BoardPreset.CreateCommunityChestDeck();
        var chanceDeck = BoardPreset.CreateChanceDeck();

        // Create game controller
        _game = new GameController(board, players, dices, communityChestDeck, chanceDeck);

        // Subscribe to events
        _game.OnMessage += (msg) => _view.ShowMessage(msg);
        _game.OnDiceRolled += (player, d1, d2) => _view.ShowDiceRoll(d1, d2);
        _game.OnCardDrawn += (card) => _view.ShowCard(card);
        _game.OnPlayerBankrupt += (player) => _view.ShowWarning($"{player.Name} is bankrupt!");
        _game.OnPlayerWins += (player) => _view.ShowGameOver(player);

        // Start game
        _game.StartGame();

        // Main game loop
        RunGameLoop();
    }

    private static List<IPlayer> SetupPlayers()
    {
        Player.ResetPlayerCount();
        var players = new List<IPlayer>();

        _view!.ClearScreen();
        Console.WriteLine("=== PLAYER SETUP ===\n");

        Console.Write("How many players? (2-4): ");
        int numPlayers;
        while (!int.TryParse(Console.ReadLine(), out numPlayers) || numPlayers < 2 || numPlayers > 4)
        {
            Console.Write("Please enter a number between 2 and 4: ");
        }

        for (int i = 0; i < numPlayers; i++)
        {
            Console.Write($"Enter name for Player {i + 1}: ");
            string name = Console.ReadLine() ?? $"Player {i + 1}";
            if (string.IsNullOrWhiteSpace(name))
                name = $"Player {i + 1}";
            
            players.Add(new Player(name));
        }

        return players;
    }

    private static void RunGameLoop()
    {
        while (!_game!.IsGameOver)
        {
            var currentPlayer = _game.CurrentPlayer;

            // Skip bankrupt players
            if (currentPlayer.State == PlayerState.Bankrupt)
            {
                _game.NextTurn();
                continue;
            }

            // Display current game state
            _view!.ClearScreen();
            _view.DrawBoard(_game.Board, _game.Players.ToList());
            _view.ShowAllPlayersInfo(_game.Players.ToList());
            _view.ShowPlayerInfo(currentPlayer);

            Console.WriteLine($"\n>>> {currentPlayer.Name}'s Turn <<<");

            // Handle jail
            if (currentPlayer.State == PlayerState.InJail)
            {
                HandleJailTurn();
                if (currentPlayer.State == PlayerState.InJail)
                {
                    _view.WaitForKeyPress();
                    _game.NextTurn();
                    continue;
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
                    _view.ShowMenu("Actions", new List<string>
                    {
                        "Roll Dice",
                        "View Properties",
                        "Manage Properties",
                        "Trade",
                        "End Turn (forfeit roll)"
                    });

                    int choice = _view.GetPlayerChoice(5);

                    switch (choice)
                    {
                        case 1:
                            var (dice1, dice2) = _game.RollDice();
                            rolled = true;

                            if (dice1 == dice2)
                            {
                                consecutiveDoubles++;
                                if (consecutiveDoubles >= 3)
                                {
                                    _view.ShowWarning("Three doubles in a row! Go to Jail!");
                                    _game.SendToJail();
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

                            if (currentPlayer.State != PlayerState.InJail)
                            {
                                _game.MovePlayer(dice1 + dice2);
                                _game.OnLand();

                                // Handle property purchase
                                HandlePropertyPurchase();
                            }
                            break;

                        case 2:
                            ViewProperties();
                            break;

                        case 3:
                            ManageProperties();
                            break;

                        case 4:
                            HandleTrade();
                            break;

                        case 5:
                            rolled = true;
                            canRollAgain = false;
                            break;
                    }
                }
            } while (canRollAgain && currentPlayer.State != PlayerState.InJail && !_game.IsGameOver);

            if (!_game.IsGameOver)
            {
                // Post-turn actions
                PostTurnActions();
                _view.WaitForKeyPress();
                _game.NextTurn();
            }
        }

        // Game over
        if (_game.Winner != null)
        {
            _view!.ShowGameOver(_game.Winner);
        }
        
        Console.WriteLine("\nThank you for playing Monopoly!");
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    private static void HandleJailTurn()
    {
        var currentPlayer = _game!.CurrentPlayer;
        
        _view!.ShowWarning($"{currentPlayer.Name} is in Jail! (Turn {currentPlayer.JailTurns + 1}/3)");

        var options = new List<string>
        {
            "Try to roll doubles",
            $"Pay ${50} to get out"
        };

        if (currentPlayer.HasGetOutOfJailCard)
        {
            options.Add("Use Get Out of Jail Free card");
        }

        _view.ShowMenu("Jail Options", options);
        int choice = _view.GetPlayerChoice(options.Count);

        switch (choice)
        {
            case 1:
                _game.TryRollDoublesForJail();
                break;
            case 2:
                _game.PayJailFee();
                break;
            case 3:
                _game.UseGetOutOfJailCard();
                break;
        }
    }

    private static void HandlePropertyPurchase()
    {
        var tile = _game!.CurrentPlayer.CurrentTile;
        if (tile?.Asset == null || tile.Asset.Owner != null)
            return;

        var asset = tile.Asset;
        var player = _game.CurrentPlayer;

        _view!.ShowPropertyDetails(asset);

        if (player.Money.Balance >= asset.Value)
        {
            if (_view.GetYesNo($"Do you want to buy {asset.Name} for ${asset.Value}?"))
            {
                _game.PlayerBuyAsset(asset);
            }
        }
        else
        {
            _view.ShowWarning($"You don't have enough money to buy {asset.Name}.");
        }
    }

    private static void ViewProperties()
    {
        var player = _game!.CurrentPlayer;

        if (player.Assets.Count == 0)
        {
            _view!.ShowMessage("You don't own any properties.");
            _view.WaitForKeyPress();
            return;
        }

        _view!.ShowMessage("\n=== Your Properties ===");
        for (int i = 0; i < player.Assets.Count; i++)
        {
            var asset = player.Assets[i];
            string status = asset.AssetsCondition == AssetsCondition.MORTGAGED ? " [MORTGAGED]" : "";
            string houses = asset.AmountHouse > 0 ? $" - {asset.AmountHouse} houses" : "";
            Console.WriteLine($"  [{i + 1}] {asset.Name} - ${asset.Value}{status}{houses}");
        }

        Console.Write("\nEnter property number to view details (0 to go back): ");
        if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= player.Assets.Count)
        {
            _view.ShowPropertyDetails(player.Assets[choice - 1]);
        }
        _view.WaitForKeyPress();
    }

    private static void ManageProperties()
    {
        var player = _game!.CurrentPlayer;

        if (player.Assets.Count == 0)
        {
            _view!.ShowMessage("You don't own any properties.");
            _view.WaitForKeyPress();
            return;
        }

        _view!.ShowMenu("Property Management", new List<string>
        {
            "Build House",
            "Sell House",
            "Mortgage Property",
            "Unmortgage Property",
            "Go Back"
        });

        int choice = _view.GetPlayerChoice(5);

        switch (choice)
        {
            case 1:
                BuildHouse();
                break;
            case 2:
                SellHouse();
                break;
            case 3:
                MortgageProperty();
                break;
            case 4:
                UnmortgageProperty();
                break;
            case 5:
                return;
        }
    }

    private static void BuildHouse()
    {
        var player = _game!.CurrentPlayer;
        var buildableProperties = player.Assets
            .Where(a => a.TypeAsset == TypeAsset.REAL_ESTATE && 
                        a.AmountHouse < 5 && 
                        a.AssetsCondition == AssetsCondition.NORMAL)
            .ToList();

        if (buildableProperties.Count == 0)
        {
            _view!.ShowMessage("No properties available for building.");
            _view.WaitForKeyPress();
            return;
        }

        _view!.ShowMessage("\n=== Build House ===");
        for (int i = 0; i < buildableProperties.Count; i++)
        {
            var asset = buildableProperties[i];
            Console.WriteLine($"  [{i + 1}] {asset.Name} - House cost: ${asset.HouseCost} - Current: {asset.AmountHouse} houses");
        }

        Console.Write("\nSelect property (0 to cancel): ");
        if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= buildableProperties.Count)
        {
            _game.PlayerAddHouse(buildableProperties[choice - 1]);
        }
        _view.WaitForKeyPress();
    }

    private static void SellHouse()
    {
        var player = _game!.CurrentPlayer;
        var sellableProperties = player.Assets
            .Where(a => a.AmountHouse > 0)
            .ToList();

        if (sellableProperties.Count == 0)
        {
            _view!.ShowMessage("No houses to sell.");
            _view.WaitForKeyPress();
            return;
        }

        _view!.ShowMessage("\n=== Sell House ===");
        for (int i = 0; i < sellableProperties.Count; i++)
        {
            var asset = sellableProperties[i];
            Console.WriteLine($"  [{i + 1}] {asset.Name} - Houses: {asset.AmountHouse} - Sell price: ${asset.HouseCost / 2}");
        }

        Console.Write("\nSelect property (0 to cancel): ");
        if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= sellableProperties.Count)
        {
            _game.PlayerSellHouse(sellableProperties[choice - 1]);
        }
        _view.WaitForKeyPress();
    }

    private static void MortgageProperty()
    {
        var player = _game!.CurrentPlayer;
        var mortgageableProperties = player.Assets
            .Where(a => a.AssetsCondition == AssetsCondition.NORMAL && a.AmountHouse == 0)
            .ToList();

        if (mortgageableProperties.Count == 0)
        {
            _view!.ShowMessage("No properties available to mortgage.");
            _view.WaitForKeyPress();
            return;
        }

        _view!.ShowMessage("\n=== Mortgage Property ===");
        for (int i = 0; i < mortgageableProperties.Count; i++)
        {
            var asset = mortgageableProperties[i];
            Console.WriteLine($"  [{i + 1}] {asset.Name} - Mortgage value: ${asset.GetMortgageValue()}");
        }

        Console.Write("\nSelect property (0 to cancel): ");
        if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= mortgageableProperties.Count)
        {
            _game.PlayerMortgageAsset(player, mortgageableProperties[choice - 1]);
        }
        _view.WaitForKeyPress();
    }

    private static void UnmortgageProperty()
    {
        var player = _game!.CurrentPlayer;
        var mortgagedProperties = player.Assets
            .Where(a => a.AssetsCondition == AssetsCondition.MORTGAGED)
            .ToList();

        if (mortgagedProperties.Count == 0)
        {
            _view!.ShowMessage("No mortgaged properties.");
            _view.WaitForKeyPress();
            return;
        }

        _view!.ShowMessage("\n=== Unmortgage Property ===");
        for (int i = 0; i < mortgagedProperties.Count; i++)
        {
            var asset = mortgagedProperties[i];
            Console.WriteLine($"  [{i + 1}] {asset.Name} - Unmortgage cost: ${asset.GetUnmortgageValue()}");
        }

        Console.Write("\nSelect property (0 to cancel): ");
        if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= mortgagedProperties.Count)
        {
            _game.PlayerUnmortgageAsset(player, mortgagedProperties[choice - 1]);
        }
        _view.WaitForKeyPress();
    }

    private static void HandleTrade()
    {
        var currentPlayer = _game!.CurrentPlayer;
        var otherPlayers = _game.Players
            .Where(p => p != currentPlayer && p.State != PlayerState.Bankrupt)
            .ToList();

        if (otherPlayers.Count == 0)
        {
            _view!.ShowMessage("No other players to trade with.");
            _view.WaitForKeyPress();
            return;
        }

        _view!.ShowMessage("\n=== Trade ===");
        _view.ShowMessage("Select player to trade with:");
        for (int i = 0; i < otherPlayers.Count; i++)
        {
            var p = otherPlayers[i];
            Console.WriteLine($"  [{i + 1}] {p.Name} - ${p.Money.Balance} - {p.Assets.Count} properties");
        }

        Console.Write("\nSelect player (0 to cancel): ");
        if (!int.TryParse(Console.ReadLine(), out int choice) || choice <= 0 || choice > otherPlayers.Count)
        {
            return;
        }

        var targetPlayer = otherPlayers[choice - 1];

        // Get properties to offer
        var offeredProperties = SelectPropertiesFromPlayer(currentPlayer, "Select your properties to offer");
        Console.Write("Enter amount of money to offer: $");
        int.TryParse(Console.ReadLine(), out int offeredMoney);

        // Get properties to request
        var requestedProperties = SelectPropertiesFromPlayer(targetPlayer, $"Select {targetPlayer.Name}'s properties you want");
        Console.Write("Enter amount of money to request: $");
        int.TryParse(Console.ReadLine(), out int requestedMoney);

        // Show trade summary
        _view.ShowTradeOffer(currentPlayer, targetPlayer, offeredProperties, offeredMoney, requestedProperties, requestedMoney);

        // In a real game, this would require the other player to accept
        // For simplicity, we'll auto-accept if AI or prompt for hotseat
        if (_view.GetYesNo($"Does {targetPlayer.Name} accept this trade?"))
        {
            _game.PlayerProposeTrade(currentPlayer, targetPlayer, offeredProperties, offeredMoney, requestedProperties, requestedMoney);
        }
        else
        {
            _view.ShowMessage("Trade rejected.");
        }

        _view.WaitForKeyPress();
    }

    private static List<IAsset> SelectPropertiesFromPlayer(IPlayer player, string prompt)
    {
        var selected = new List<IAsset>();

        if (player.Assets.Count == 0)
        {
            _view!.ShowMessage($"{player.Name} has no properties.");
            return selected;
        }

        _view!.ShowMessage($"\n{prompt} (enter numbers separated by comma, or 0 for none):");
        for (int i = 0; i < player.Assets.Count; i++)
        {
            Console.WriteLine($"  [{i + 1}] {player.Assets[i].Name}");
        }

        Console.Write("Selection: ");
        string? input = Console.ReadLine();
        
        if (string.IsNullOrEmpty(input) || input == "0")
            return selected;

        foreach (var part in input.Split(','))
        {
            if (int.TryParse(part.Trim(), out int idx) && idx > 0 && idx <= player.Assets.Count)
            {
                selected.Add(player.Assets[idx - 1]);
            }
        }

        return selected;
    }

    private static void PostTurnActions()
    {
        // Check if current player is bankrupt
        if (_game!.CurrentPlayer.Money.Balance < 0)
        {
            _view!.ShowWarning($"{_game.CurrentPlayer.Name} has negative balance!");
            
            // Allow player to mortgage properties or sell houses
            while (_game.CurrentPlayer.Money.Balance < 0 && _game.CurrentPlayer.Assets.Count > 0)
            {
                _view.ShowMessage($"Current balance: ${_game.CurrentPlayer.Money.Balance}");
                _view.ShowMenu("You must raise funds!", new List<string>
                {
                    "Sell House",
                    "Mortgage Property",
                    "Declare Bankruptcy"
                });

                int choice = _view.GetPlayerChoice(3);
                switch (choice)
                {
                    case 1:
                        SellHouse();
                        break;
                    case 2:
                        MortgageProperty();
                        break;
                    case 3:
                        _game.CheckIsBankrupt(_game.CurrentPlayer);
                        return;
                }
            }

            if (_game.CurrentPlayer.Money.Balance < 0)
            {
                _game.CheckIsBankrupt(_game.CurrentPlayer);
            }
        }
    }
}
