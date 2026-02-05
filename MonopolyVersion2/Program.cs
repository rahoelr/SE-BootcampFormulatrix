using ConsoleMonopolyApp.Views;
using MonopolyApp.Controllers;
using MonopolyApp.Models;
using MonopolyApp.Interfaces;
using MonopolyApp.Enums;
using MonopolyApp.Data;

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
            _view.ShowError("Perlu minimal 2 pemain untuk bermain!");
            return;
        }

        // Setup game components
        var board = SetupBoard.CreateStandardBoard();
        var dices = new List<IDice> { new Dice(6), new Dice(6) };
        var communityChestDeck = SetupBoard.CreateCommunityChestDeck();
        var chanceDeck = SetupBoard.CreateChanceDeck();

        // Create game controller
        _game = new GameController(board, players, dices, communityChestDeck, chanceDeck);

        // Subscribe to events
        _game.OnMessage += (msg) => _view.ShowMessage(msg);
        _game.OnDiceRolled += (player, d1, d2) => _view.ShowDiceRoll(d1, d2);
        _game.OnCardDrawn += (card) => _view.ShowCard(card);
        _game.OnPlayerBankrupt += (player) => _view.ShowWarning($"{player.Name} bangkrut!");
        _game.OnPlayerWins += (player) =>
        {
            int winnerMoney = _game.GetPlayerMoney(player);
            _view.ShowGameOver(player, winnerMoney);
        };

        // Start game
        _game.StartGame();

        // Main game loop
        RunGameLoop();
    }

    private static List<IPlayer> SetupPlayers()
    {
        var players = new List<IPlayer>();

        _view!.ClearScreen();
        Console.WriteLine("=== SETUP PEMAIN ===\n");

        Console.Write("Berapa pemain? (2-4): ");
        int numPlayers;
        while (!int.TryParse(Console.ReadLine(), out numPlayers) || numPlayers < 2 || numPlayers > 4)
        {
            Console.Write("Masukkan angka antara 2 dan 4: ");
        }

        for (int i = 0; i < numPlayers; i++)
        {
            Console.Write($"Masukkan nama Pemain {i + 1}: ");
            string name = Console.ReadLine() ?? $"Pemain {i + 1}";
            if (string.IsNullOrWhiteSpace(name))
                name = $"Pemain {i + 1}";

            players.Add(new Player(name, new Money(1500)));
        }

        return players;
    }

    private static void RunGameLoop()
    {
        while (!_game!.IsGameOver)
        {
            var currentPlayer = _game.CurrentPlayer;

            // Skip bankrupt players
            if (currentPlayer.PlayerState == PlayerState.Bankrupt)
            {
                _game.NextTurn();
                continue;
            }

            // Display current game state
            _view!.ClearScreen();
            _view.DrawBoard(_game.Board, _game.Players);

            var playerMoneyDict = new Dictionary<IPlayer, int>();
            foreach (var player in _game.Players)
            {
                playerMoneyDict[player] = _game.GetPlayerMoney(player);
            }

            _view.ShowAllPlayersInfo(_game.Players, playerMoneyDict);
            _view.ShowPlayerInfo(currentPlayer, _game.GetPlayerMoney(currentPlayer));

            Console.WriteLine($"\n>>> Giliran {currentPlayer.Name} <<<");

            // Handle jail
            if (currentPlayer.PlayerState == PlayerState.InJail)
            {
                HandleJailTurn();
                if (currentPlayer.PlayerState == PlayerState.InJail)
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
                            (dice1, dice2) = _game.RollDices();
                            rolled = true;

                            if (dice1 == dice2)
                            {
                                consecutiveDoubles++;
                                if (consecutiveDoubles >= 3)
                                {
                                    _view.ShowWarning("Tiga kali ganda berturut-turut! Masuk penjara!");
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

                            if (currentPlayer.PlayerState != PlayerState.InJail)
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
            } while (canRollAgain && currentPlayer.PlayerState != PlayerState.InJail && !_game.IsGameOver);

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
                int winnerMoney = _game.GetPlayerMoney(_game.Winner);
                _view!.ShowGameOver(_game.Winner, winnerMoney);
            }

            Console.WriteLine("\nTerima kasih sudah bermain Monopoly!");
            Console.WriteLine("Tekan tombol apa saja untuk keluar...");
            Console.ReadKey();
        }

    private static void HandleJailTurn()
    {
        var currentPlayer = _game!.CurrentPlayer;
        
        // Increment jail turns dan cek apakah sudah 3 giliran
        bool canChoose = _game.HandleJailTurn();
        if (!canChoose)
        {
            // Sudah 3 giliran atau state bukan InJail, sudah dihandle di GameController
            return;
        }
        
        int jailTurns = _game.GetJailTurns(currentPlayer);
        _view!.ShowWarning($"{currentPlayer.Name} di Penjara! (Giliran {jailTurns}/3)");

        var options = new List<string>
        {
            "Coba lempar ganda",
            "Bayar $50 untuk keluar"
        };

        if (_game.HasGetOutOfJailCard(currentPlayer))
        {
            options.Add("Gunakan kartu Bebas Penjara");
        }

        _view.ShowMenu("Opsi Penjara", options);
        int choice = _view.GetPlayerChoice(options.Count);

        switch (choice)
        {
            case 1:
                _game.TryRollDoublesInJail();
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
        var asset = _game.TileAssets.ContainsKey(tile) ? _game.TileAssets[tile] : null;

        if (asset == null || asset.Owner != null)
            return;

        var player = _game.CurrentPlayer;

        _view!.ShowPropertyDetails(asset);

        int playerMoney = _game.GetPlayerMoney(player);
        if (playerMoney >= asset.Value)
        {
            if (_view.GetYesNo($"Beli {asset.Name} seharga ${asset.Value}?"))
            {
                _game.PlayerBuyAsset(asset);
            }
        }
        else
        {
            _view.ShowWarning($"Uang tidak cukup untuk membeli {asset.Name}.");
        }
    }

    private static void ViewProperties()
    {
        var player = _game!.CurrentPlayer;

        if (player.Assets.Count == 0)
        {
            _view!.ShowMessage("Anda tidak memiliki properti.");
            _view.WaitForKeyPress();
            return;
        }

        _view!.ShowMessage("\n=== Properti Anda ===");
        for (int i = 0; i < player.Assets.Count; i++)
        {
            var asset = player.Assets[i];
            string status = asset.AssetCondition == AssetCondition.Mortgage ? " [MORTGAGE]" : "";
            string houses = asset.AmountHouse > 0 ? $" - {asset.AmountHouse} rumah" : "";
            Console.WriteLine($"  [{i + 1}] {asset.Name} - ${asset.Value}{status}{houses}");
        }

        Console.Write("\nMasukkan nomor properti untuk detail (0 untuk kembali): ");
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
            _view!.ShowMessage("Anda tidak memiliki properti.");
            _view.WaitForKeyPress();
            return;
        }

        _view!.ShowMenu("Kelola Properti", new List<string>
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
            .Where(a => a.TypeAsset == TypeAsset.RealEstate &&
                        a.AmountHouse < 5 &&
                        a.AssetCondition == AssetCondition.Normal)
            .ToList();

        if (buildableProperties.Count == 0)
        {
            _view!.ShowMessage("Tidak ada properti untuk dibangun.");
            _view.WaitForKeyPress();
            return;
        }

        _view!.ShowMessage("\n=== Bangun Rumah ===");
        for (int i = 0; i < buildableProperties.Count; i++)
        {
            var asset = buildableProperties[i];
            int houseCost = asset.Value / 2;
            Console.WriteLine($"  [{i + 1}] {asset.Name} - Biaya rumah: ${houseCost} - Saat ini: {asset.AmountHouse} rumah");
        }

        Console.Write("\nPilih properti (0 untuk batal): ");
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
            _view!.ShowMessage("Tidak ada rumah untuk dijual.");
            _view.WaitForKeyPress();
            return;
        }

        _view!.ShowMessage("\n=== Jual Rumah ===");
        for (int i = 0; i < sellableProperties.Count; i++)
        {
            var asset = sellableProperties[i];
            int sellPrice = asset.Value / 4;
            Console.WriteLine($"  [{i + 1}] {asset.Name} - Rumah: {asset.AmountHouse} - Harga jual: ${sellPrice}");
        }

        Console.Write("\nPilih properti (0 untuk batal): ");
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
            .Where(a => a.AssetCondition == AssetCondition.Normal && a.AmountHouse == 0)
            .ToList();

        if (mortgageableProperties.Count == 0)
        {
            _view!.ShowMessage("Tidak ada properti untuk di-mortgage.");
            _view.WaitForKeyPress();
            return;
        }

        _view!.ShowMessage("\n=== Mortgage Properti ===");
        for (int i = 0; i < mortgageableProperties.Count; i++)
        {
            var asset = mortgageableProperties[i];
            int mortgageValue = asset.Value / 2;
            Console.WriteLine($"  [{i + 1}] {asset.Name} - Nilai mortgage: ${mortgageValue}");
        }

        Console.Write("\nPilih properti (0 untuk batal): ");
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
            .Where(a => a.AssetCondition == AssetCondition.Mortgage)
            .ToList();

        if (mortgagedProperties.Count == 0)
        {
            _view!.ShowMessage("Tidak ada properti yang di-mortgage.");
            _view.WaitForKeyPress();
            return;
        }

        _view!.ShowMessage("\n=== Unmortgage Properti ===");
        for (int i = 0; i < mortgagedProperties.Count; i++)
        {
            var asset = mortgagedProperties[i];
            int unmortgageValue = (asset.Value / 2) + ((asset.Value / 2) / 10);
            Console.WriteLine($"  [{i + 1}] {asset.Name} - Biaya unmortgage: ${unmortgageValue}");
        }

        Console.Write("\nPilih properti (0 untuk batal): ");
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
            .Where(p => p != currentPlayer && p.PlayerState != PlayerState.Bankrupt)
            .ToList();

        if (otherPlayers.Count == 0)
        {
            _view!.ShowMessage("Tidak ada pemain lain untuk berdagang.");
            _view.WaitForKeyPress();
            return;
        }

        _view!.ShowMessage("\n=== Perdagangan ===");
        _view.ShowMessage("Pilih pemain untuk berdagang:");
        for (int i = 0; i < otherPlayers.Count; i++)
        {
            var p = otherPlayers[i];
            int playerMoney = _game.GetPlayerMoney(p);
            Console.WriteLine($"  [{i + 1}] {p.Name} - ${playerMoney} - {p.Assets.Count} properti");
        }

        Console.Write("\nPilih pemain (0 untuk batal): ");
        if (!int.TryParse(Console.ReadLine(), out int choice) || choice <= 0 || choice > otherPlayers.Count)
        {
            return;
        }

        var targetPlayer = otherPlayers[choice - 1];

        // Get properties to offer
        var offeredProperties = SelectPropertiesFromPlayer(currentPlayer, "Pilih properti Anda untuk ditawarkan");
        Console.Write("Masukkan jumlah uang untuk ditawarkan: $");
        int.TryParse(Console.ReadLine(), out int offeredMoney);

        // Get properties to request
        var requestedProperties = SelectPropertiesFromPlayer(targetPlayer, $"Pilih properti {targetPlayer.Name} yang Anda inginkan");
        Console.Write("Masukkan jumlah uang yang diminta: $");
        int.TryParse(Console.ReadLine(), out int requestedMoney);

        // Show trade summary
        _view.ShowTradeOffer(currentPlayer, targetPlayer, offeredProperties, offeredMoney, requestedProperties, requestedMoney);

        if (_view.GetYesNo($"Apakah {targetPlayer.Name} menerima perdagangan ini?"))
        {
            _game.PlayerProposeTrade(currentPlayer, targetPlayer, offeredProperties, offeredMoney, requestedProperties, requestedMoney);
        }
        else
        {
            _view.ShowMessage("Perdagangan ditolak.");
        }

        _view.WaitForKeyPress();
    }

    private static List<IAsset> SelectPropertiesFromPlayer(IPlayer player, string prompt)
    {
        var selected = new List<IAsset>();

        if (player.Assets.Count == 0)
        {
            _view!.ShowMessage($"{player.Name} tidak memiliki properti.");
            return selected;
        }

        _view!.ShowMessage($"\n{prompt} (masukkan nomor dipisah koma, atau 0 untuk tidak ada):");
        for (int i = 0; i < player.Assets.Count; i++)
        {
            Console.WriteLine($"  [{i + 1}] {player.Assets[i].Name}");
        }

        Console.Write("Pilihan: ");
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
        var currentPlayer = _game!.CurrentPlayer;
        int playerMoney = _game.GetPlayerMoney(currentPlayer);

        // Check if current player is bankrupt
        if (playerMoney < 0)
        {
            _view!.ShowWarning($"{currentPlayer.Name} memiliki saldo negatif!");

            // Allow player to mortgage properties or sell houses
            while (_game.GetPlayerMoney(currentPlayer) < 0 && currentPlayer.Assets.Count > 0)
            {
                _view.ShowMessage($"Saldo saat ini: ${_game.GetPlayerMoney(currentPlayer)}");
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
                        SellHouse();
                        break;
                    case 2:
                        MortgageProperty();
                        break;
                    case 3:
                        _game.CheckIsBankrupt(currentPlayer);
                        return;
                }
            }

            if (_game.GetPlayerMoney(currentPlayer) < 0)
            {
                _game.CheckIsBankrupt(currentPlayer);
            }
        }
    }
}