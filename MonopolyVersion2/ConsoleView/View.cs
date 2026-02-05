using MonopolyApp.Interfaces;
using MonopolyApp.Enums;

namespace ConsoleMonopolyApp.Views;

public class ConsoleView
{
    private const int TILE_WIDTH = 12;
    private const int TILE_HEIGHT = 3;

    public void ClearScreen()
    {
        Console.Clear();
    }

    public void DrawBoard(IBoard board, List<IPlayer> players)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        
        int boardWidth = 11;
        int boardHeight = 11;

        // Draw the board
        for (int y = 0; y < boardHeight; y++)
        {
            // Draw 3 lines per tile row
            for (int line = 0; line < TILE_HEIGHT; line++)
            {
                for (int x = 0; x < boardWidth; x++)
                {
                    var tile = board.Path.FirstOrDefault(t => t.Pos.X == x && t.Pos.Y == y);
                    
                    if (tile != null)
                    {
                        DrawTileLine(tile, line, players);
                    }
                    else if (y > 0 && y < boardHeight - 1 && x > 0 && x < boardWidth - 1)
                    {
                        // Empty center area
                        DrawCenterArea(x, y, line, boardWidth, boardHeight);
                    }
                    else
                    {
                        Console.Write(new string(' ', TILE_WIDTH));
                    }
                }
                Console.WriteLine();
            }
        }
    }

    private void DrawTileLine(ITile tile, int line, List<IPlayer> players)
    {
        // Get players on this tile
        var playersOnTile = players.Where(p => p.CurrentTile == tile && p.PlayerState != PlayerState.Bankrupt).ToList();

        switch (line)
        {
            case 0:
                // Top border
                Console.Write("┌");
                Console.Write(new string('─', TILE_WIDTH - 2));
                Console.Write("┐");
                break;

            case 1:
                // Tile content
                Console.Write("│");
                
                string displayName = GetShortName(tile.Name);
                string playerMarkers = GetPlayerMarkers(playersOnTile);
                string content = $"{displayName}{playerMarkers}";
                
                if (content.Length > TILE_WIDTH - 2)
                    content = content.Substring(0, TILE_WIDTH - 2);
                
                Console.Write(content.PadRight(TILE_WIDTH - 2));
                Console.Write("│");
                break;

            case 2:
                // Bottom info (price or special)
                Console.Write("│");
                
                string info = GetSpecialTileInfo(tile);

                if (info.Length > TILE_WIDTH - 2)
                    info = info.Substring(0, TILE_WIDTH - 2);
                
                Console.Write(info.PadRight(TILE_WIDTH - 2));
                Console.Write("│");
                break;
        }
    }

    private void DrawCenterArea(int x, int y, int line, int boardWidth, int boardHeight)
    {
        int centerX = boardWidth / 2;
        int centerY = boardHeight / 2;

        if (y == centerY && line == 1)
        {
            if (x == centerX - 1)
                Console.Write(" MONOPOLY   ");
            else if (x == centerX)
                Console.Write("            ");
            else if (x == centerX + 1)
                Console.Write("            ");
            else
                Console.Write(new string(' ', TILE_WIDTH));
        }
        else
        {
            Console.Write(new string(' ', TILE_WIDTH));
        }
    }

    private string GetShortName(string name)
    {
        if (name.Length <= 8) return name;

        // Common abbreviations
        var abbreviations = new Dictionary<string, string>
        {
            { "Stasiun", "Stn" },
            { "Perusahaan", "Per" },
            { "Listrik", "List" },
            { "Pajak", "Pjk" },
            { "Penjara", "Jail" },
            { "Parkir Gratis", "Parkir" },
            { "Kesempatan", "Ksmptn" },
            { "Dana Umum", "Dana" }
        };

        foreach (var abbr in abbreviations)
        {
            if (name.Contains(abbr.Key))
            {
                name = name.Replace(abbr.Key, abbr.Value);
            }
        }

        if (name.Length > 8)
        {
            name = name.Substring(0, 8);
        }

        return name;
    }

    private string GetPlayerMarkers(List<IPlayer> players)
    {
        if (players.Count == 0) return "";
        
        // Gunakan initial huruf pertama dari nama player
        var markers = players.Select(p => p.Name[0].ToString()).ToList();
        return " " + string.Join("", markers);
    }

    private string GetSpecialTileInfo(ITile tile)
    {
        return tile.EffectType switch
        {
            EffectType.Go => "+$200",
            EffectType.Tax => tile.Name.Contains("Mewah") ? "-$100" : "-$200",
            EffectType.CommunityChest => "DANA",
            EffectType.Chance => "KSMPTN",
            EffectType.GoToJail => "JAIL!",
            EffectType.FreeParking => "PARKIR",
            _ => ""
        };
    }

    public void ShowPlayerInfo(IPlayer player, int playerMoney)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"═══ {player.Name} [{player.Name[0]}] ═══");
        Console.ResetColor();
        
        Console.WriteLine($"  Uang: ${playerMoney}");
        Console.WriteLine($"  Posisi: {player.CurrentTile?.Name ?? "Unknown"} (Tile {player.PathIndex})");
        Console.WriteLine($"  Status: {player.PlayerState}");
        
        if (player.Assets.Count > 0)
        {
            Console.WriteLine($"  Properti ({player.Assets.Count}):");
            foreach (var asset in player.Assets)
            {
                string status = asset.AssetCondition == AssetCondition.Mortgage ? " [M]" : "";
                string houses = asset.AmountHouse > 0 ? $" [{new string('H', Math.Min(asset.AmountHouse, 4))}{(asset.AmountHouse == 5 ? "!" : "")}]" : "";
                Console.WriteLine($"    - {asset.Name}{status}{houses}");
            }
        }
    }

    public void ShowAllPlayersInfo(List<IPlayer> players, Dictionary<IPlayer, int> playerMoney)
    {
        Console.WriteLine("\n╔═══════════════════════════════════════════╗");
        Console.WriteLine("║           STATUS PEMAIN                   ║");
        Console.WriteLine("╠═══════════════════════════════════════════╣");
        
        foreach (var player in players)
        {
            string status = player.PlayerState == PlayerState.Bankrupt ? " (BANGKRUT)" : "";
            string jail = player.PlayerState == PlayerState.InJail ? " [PENJARA]" : "";
            int money = playerMoney.ContainsKey(player) ? playerMoney[player] : 0;
            Console.WriteLine($"║ {player.Name[0]} {player.Name,-12} ${money,-8} {player.Assets.Count} props{status}{jail}");
        }
        
        Console.WriteLine("╚═══════════════════════════════════════════╝");
    }

    public void ShowMessage(string message)
    {
        Console.WriteLine(message);
    }

    public void ShowError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"ERROR: {message}");
        Console.ResetColor();
    }

    public void ShowSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    public void ShowWarning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    public void ShowDiceRoll(int dice1, int dice2)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("  ╔═══╗ ╔═══╗");
        Console.WriteLine($"  ║ {dice1} ║ ║ {dice2} ║");
        Console.WriteLine("  ╚═══╝ ╚═══╝");
        Console.WriteLine($"  Total: {dice1 + dice2}");
        if (dice1 == dice2)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  GANDA!");
        }
        Console.ResetColor();
    }

    public void ShowCard(ICard card)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("╔══════════════════════════════════════╗");
        Console.WriteLine($"║  {card.Name,-34}  ║");
        Console.WriteLine("╠══════════════════════════════════════╣");
        
        // Word wrap description
        string desc = card?.Description ?? "";
        while (desc.Length > 34)
        {
            Console.WriteLine($"║  {desc.Substring(0, 34)}  ║");
            desc = desc.Substring(34);
        }
        Console.WriteLine($"║  {desc,-34}  ║");
        Console.WriteLine("╚══════════════════════════════════════╝");
        Console.ResetColor();
    }

    public void ShowMenu(string title, List<string> options)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"═══ {title} ═══");
        Console.ResetColor();
        
        for (int i = 0; i < options.Count; i++)
        {
            Console.WriteLine($"  [{i + 1}] {options[i]}");
        }
        Console.Write("\nPilihan Anda: ");
    }

    public int GetPlayerChoice(int maxOptions)
    {
        while (true)
        {
            if (int.TryParse(Console.ReadLine(), out int choice) && choice >= 1 && choice <= maxOptions)
            {
                return choice;
            }
            Console.Write($"Masukkan angka antara 1 dan {maxOptions}: ");
        }
    }

    public string GetPlayerInput(string prompt)
    {
        Console.Write(prompt);
        return Console.ReadLine() ?? "";
    }

    public bool GetYesNo(string prompt)
    {
        Console.Write($"{prompt} (y/t): ");
        while (true)
        {
            string input = Console.ReadLine()?.ToLower() ?? "";
            if (input == "y" || input == "yes") return true;
            if (input == "t" || input == "tidak") return false;
            Console.Write("Masukkan 'y' atau 't': ");
        }
    }

    public void ShowPropertyDetails(IAsset asset)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("╔══════════════════════════════════════╗");
        Console.WriteLine($"║  {asset.Name,-34}  ║");
        Console.WriteLine("╠══════════════════════════════════════╣");
        Console.ResetColor();
        
        Console.WriteLine($"║  Harga: ${asset.Value,-28}  ║");
        Console.WriteLine($"║  Tipe: {asset.TypeAsset,-29}  ║");
        
        if (asset.TypeAsset == TypeAsset.RealEstate)
        {
            int houseCost = asset.Value / 2;
            int baseRent = asset.Value / 10;
            Console.WriteLine($"║  Biaya Rumah: ${houseCost,-23}  ║");
            Console.WriteLine($"║  Sewa Dasar: ${baseRent,-24}  ║");
        }
        
        int mortgageValue = asset.Value / 2;
        Console.WriteLine($"║  Nilai Mortgage: ${mortgageValue,-19}  ║");
        
        if (asset.Owner != null)
        {
            Console.WriteLine($"║  Pemilik: {asset.Owner.Name,-28}  ║");
            Console.WriteLine($"║  Rumah: {asset.AmountHouse,-27}  ║");
            Console.WriteLine($"║  Status: {asset.AssetCondition,-27}  ║");
        }
        
        Console.WriteLine("╚══════════════════════════════════════╝");
    }

    public void ShowTradeOffer(IPlayer from, IPlayer to, 
                                List<IAsset> offerFrom, int moneyFrom,
                                List<IAsset> offerTo, int moneyTo)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔══════════════════════════════════════════════════════╗");
        Console.WriteLine("║                 PENAWARAN PERDAGANGAN                 ║");
        Console.WriteLine("╠══════════════════════════════════════════════════════╣");
        Console.ResetColor();
        
        Console.WriteLine($"║  {from.Name} menawarkan:                              ");
        foreach (var asset in offerFrom)
        {
            Console.WriteLine($"║    - {asset.Name}");
        }
        if (moneyFrom > 0)
            Console.WriteLine($"║    + ${moneyFrom}");
        
        Console.WriteLine("║                                                       ");
        Console.WriteLine($"║  Untuk ditukar dengan milik {to.Name}:                ");
        foreach (var asset in offerTo)
        {
            Console.WriteLine($"║    - {asset.Name}");
        }
        if (moneyTo > 0)
            Console.WriteLine($"║    + ${moneyTo}");
        
        Console.WriteLine("╚══════════════════════════════════════════════════════╝");
    }

    public void ShowGameOver(IPlayer winner, int winnerMoney)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("TAMAT !!!");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n    PEMENANG: {winner.Name}!");
        Console.WriteLine($"    Total Uang: ${winnerMoney}");
        Console.WriteLine($"    Properti: {winner.Assets.Count}");
        Console.ResetColor();
    }

    public void ShowWelcome()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Monopoly Gaskeunnnn!!!");
        Console.ResetColor();
        Console.WriteLine("\n    Tekan tombol apa saja untuk mulai...");
        Console.ReadKey();
    }

    public void WaitForKeyPress()
    {
        Console.WriteLine("\nTekan tombol apa saja untuk lanjut...");
        Console.ReadKey();
    }
}