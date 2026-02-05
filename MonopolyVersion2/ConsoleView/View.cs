using MonopolyApp.Interfaces;
using MonopolyApp.Enums;

namespace ConsoleMonopolyApp.Views;

public class ConsoleView
{
    private const int TILE_WIDTH = 14;
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
                Console.Write("+");
                Console.Write(new string('-', TILE_WIDTH - 2));
                Console.Write("+");
                break;

            case 1:
                // Tile content
                Console.Write("|");
                
                string displayName = GetShortName(tile.Name);
                string playerMarkers = GetPlayerMarkers(playersOnTile);
                string content = $"{displayName}{playerMarkers}";
                
                if (content.Length > TILE_WIDTH - 2)
                    content = content.Substring(0, TILE_WIDTH - 2);
                
                Console.Write(content.PadRight(TILE_WIDTH - 2));
                Console.Write("|");
                break;

            case 2:
                // Bottom info (price or special)
                Console.Write("|");
                
                string info = GetSpecialTileInfo(tile);

                if (info.Length > TILE_WIDTH - 2)
                    info = info.Substring(0, TILE_WIDTH - 2);
                
                Console.Write(info.PadRight(TILE_WIDTH - 2));
                Console.Write("|");
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
                Console.Write("   MONOPOLY   ");
            else if (x == centerX)
                Console.Write("              ");
            else if (x == centerX + 1)
                Console.Write("              ");
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
        if (name.Length <= 10) return name;

        // Common abbreviations
        var abbreviations = new Dictionary<string, string>
        {
            { "Stasiun", "Stn" },
            { "Perusahaan", "Per" },
            { "Penghasilan", "Phsln" },
            { "Kesempatan", "Ksmptn" },
            { "Dana Umum", "Dana Umum" }
        };

        foreach (var abbr in abbreviations)
        {
            if (name.Contains(abbr.Key))
            {
                name = name.Replace(abbr.Key, abbr.Value);
            }
        }

        if (name.Length > 10)
        {
            name = name.Substring(0, 10);
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
            EffectType.CommunityChest => "Dana Umum",
            EffectType.Chance => "Kesempatan",
            EffectType.GoToJail => "Ke Penjara",
            EffectType.FreeParking => "Parkir",
            _ => ""
        };
    }

    public void ShowPlayerInfo(IPlayer player, int playerMoney)
    {
        Console.WriteLine();
        Console.WriteLine($"+-------------------------------------+");
        Console.WriteLine($"|  {player.Name} [{player.Name[0]}]");
        Console.WriteLine($"+-------------------------------------+");
        
        Console.WriteLine($"|  Uang    : ${playerMoney}");
        Console.WriteLine($"|  Posisi  : {player.CurrentTile?.Name ?? "Tidak Diketahui"} (#{player.PathIndex})");
        
        string statusText = player.PlayerState switch
        {
            PlayerState.InJail => "Di Penjara",
            PlayerState.Bankrupt => "Bangkrut",
            _ => "Normal"
        };
        Console.WriteLine($"|  Status  : {statusText}");
        
        if (player.Assets.Count > 0)
        {
            Console.WriteLine($"|  Properti: {player.Assets.Count} buah");
            foreach (var asset in player.Assets)
            {
                string status = asset.AssetCondition == AssetCondition.Mortgage ? " [M]" : "";
                string houses = asset.AmountHouse > 0 ? $" [{new string('H', Math.Min(asset.AmountHouse, 4))}{(asset.AmountHouse == 5 ? "!" : "")}]" : "";
                Console.WriteLine($"|    - {asset.Name}{status}{houses}");
            }
        }
        else
        {
            Console.WriteLine($"|  Properti: -");
        }
        Console.WriteLine($"+-------------------------------------+");
    }

    public void ShowAllPlayersInfo(List<IPlayer> players, Dictionary<IPlayer, int> playerMoney)
    {
        Console.WriteLine();
        Console.WriteLine("+---+--------------+------------+-------+-------------+");
        Console.WriteLine("| # | Nama         | Uang       | Props | Status      |");
        Console.WriteLine("+---+--------------+------------+-------+-------------+");
        
        foreach (var player in players)
        {
            string status = player.PlayerState switch
            {
                PlayerState.Bankrupt => "BANGKRUT",
                PlayerState.InJail => "DI PENJARA",
                _ => "Aktif"
            };
            
            int money = playerMoney.ContainsKey(player) ? playerMoney[player] : 0;
            
            Console.WriteLine($"| {player.Name[0]} | {player.Name,-12} | ${money,-9} | {player.Assets.Count,-5} | {status,-11} |");
        }
        
        Console.WriteLine("+---+--------------+------------+-------+-------------+");
    }

    public void ShowMessage(string message)
    {
        Console.WriteLine(message);
    }

    public void ShowError(string message)
    {
        Console.WriteLine($"[ERROR] {message}");
    }

    public void ShowSuccess(string message)
    {
        Console.WriteLine(message);
    }

    public void ShowWarning(string message)
    {
        Console.WriteLine($"[!] {message}");
    }

    public void ShowDiceRoll(int dice1, int dice2)
    {
        Console.WriteLine();
        Console.WriteLine("  +---+ +---+");
        Console.WriteLine($"  | {dice1} | | {dice2} |   Total: {dice1 + dice2}");
        Console.WriteLine("  +---+ +---+");
        if (dice1 == dice2)
        {
            Console.WriteLine("  ** GANDA! **");
        }
    }

    public void ShowCard(ICard card)
    {
        Console.WriteLine();
        Console.WriteLine("+--------------------------------------+");
        Console.WriteLine($"|  {card.Name,-34}  |");
        Console.WriteLine("+--------------------------------------+");
        
        // Word wrap description
        string desc = card?.Description ?? "";
        while (desc.Length > 34)
        {
            Console.WriteLine($"|  {desc.Substring(0, 34)}  |");
            desc = desc.Substring(34);
        }
        Console.WriteLine($"|  {desc,-34}  |");
        Console.WriteLine("+--------------------------------------+");
    }

    public void ShowMenu(string title, List<string> options)
    {
        Console.WriteLine();
        Console.WriteLine($"+--- {title} ---");
        
        for (int i = 0; i < options.Count; i++)
        {
            Console.WriteLine($"|  [{i + 1}] {options[i]}");
        }
        Console.WriteLine("+------------------------");
        Console.Write("Pilihan: ");
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
        Console.WriteLine("+--------------------------------------+");
        Console.WriteLine($"|  {asset.Name,-34}  |");
        Console.WriteLine("+--------------------------------------+");
        
        Console.WriteLine($"|  Harga: ${asset.Value,-28}|");
        Console.WriteLine($"|  Tipe: {asset.TypeAsset,-29}|");
        
        if (asset.TypeAsset == TypeAsset.RealEstate)
        {
            int houseCost = asset.Value / 2;
            int baseRent = asset.Value / 10;
            Console.WriteLine($"|  Biaya Rumah: ${houseCost,-22}|");
            Console.WriteLine($"|  Sewa Dasar: ${baseRent,-23}|");
        }
        
        int mortgageValue = asset.Value / 2;
        Console.WriteLine($"|  Nilai Mortgage: ${mortgageValue,-18}|");
        
        if (asset.Owner != null)
        {
            Console.WriteLine($"|  Pemilik: {asset.Owner.Name,-26}|");
            Console.WriteLine($"|  Rumah: {asset.AmountHouse,-28}|");
            Console.WriteLine($"|  Status: {asset.AssetCondition,-27}|");
        }
        
        Console.WriteLine("+--------------------------------------+");
    }

    public void ShowTradeOffer(IPlayer from, IPlayer to, 
                                List<IAsset> offerFrom, int moneyFrom,
                                List<IAsset> offerTo, int moneyTo)
    {
        Console.WriteLine();
        Console.WriteLine("+------------------------------------------------------+");
        Console.WriteLine("|                 PENAWARAN PERDAGANGAN                |");
        Console.WriteLine("+------------------------------------------------------+");
        
        Console.WriteLine($"|  {from.Name} menawarkan:");
        foreach (var asset in offerFrom)
        {
            Console.WriteLine($"|    - {asset.Name}");
        }
        if (moneyFrom > 0)
            Console.WriteLine($"|    + ${moneyFrom}");
        
        Console.WriteLine("|");
        Console.WriteLine($"|  Untuk ditukar dengan milik {to.Name}:");
        foreach (var asset in offerTo)
        {
            Console.WriteLine($"|    - {asset.Name}");
        }
        if (moneyTo > 0)
            Console.WriteLine($"|    + ${moneyTo}");
        
        Console.WriteLine("+------------------------------------------------------+");
    }

    public void ShowGameOver(IPlayer winner, int winnerMoney)
    {
        Console.Clear();
        Console.WriteLine();
        Console.WriteLine("  +====================================+");
        Console.WriteLine("  |           GAME SELESAI!            |");
        Console.WriteLine("  +====================================+");
        Console.WriteLine($"  |  PEMENANG: {winner.Name,-24}|");
        Console.WriteLine($"  |  Total Uang: ${winnerMoney,-21}|");
        Console.WriteLine($"  |  Properti: {winner.Assets.Count,-24}|");
        Console.WriteLine("  +====================================+");
    }

    public void ShowWelcome()
    {
        Console.Clear();
        Console.WriteLine(@"
  __  __                               _       
 |  \/  | ___  _ __   ___  _ __   ___ | |_   _ 
 | |\/| |/ _ \| '_ \ / _ \| '_ \ / _ \| | | | |
 | |  | | (_) | | | | (_) | |_) | (_) | | |_| |
 |_|  |_|\___/|_| |_|\___/| .__/ \___/|_|\__, |
                          |_|            |___/ 
        ");
        Console.WriteLine("        === VERSI INDONESIA ===");
        Console.WriteLine("\n  Tekan tombol apa saja untuk mulai...");
        Console.ReadKey();
    }

    public void WaitForKeyPress()
    {
        Console.WriteLine("\n[Tekan tombol untuk lanjut...]");
        Console.ReadKey();
    }
}
