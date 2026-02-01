using ConsoleMonopolyApp.Controllers;
using ConsoleMonopolyApp.Enums;
using ConsoleMonopolyApp.Interfaces;
using ConsoleMonopolyApp.Models;

namespace ConsoleMonopolyApp.Views;

public class ConsoleView
{
    private const int TILE_WIDTH = 12;
    private const int TILE_HEIGHT = 3;

    private readonly Dictionary<int, ConsoleColor> ColorGroupColors = new()
    {
        { 1, ConsoleColor.DarkYellow },   // Brown
        { 2, ConsoleColor.Cyan },          // Light Blue
        { 3, ConsoleColor.Magenta },       // Pink
        { 4, ConsoleColor.DarkYellow },    // Orange
        { 5, ConsoleColor.Red },           // Red
        { 6, ConsoleColor.Yellow },        // Yellow
        { 7, ConsoleColor.Green },         // Green
        { 8, ConsoleColor.Blue }           // Dark Blue
    };

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
                    var tile = board.GetTileAt(x, y);
                    
                    if (tile != null)
                    {
                        DrawTileLine(tile, line, players, x == 0, x == boardWidth - 1, y == 0, y == boardHeight - 1);
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

    private void DrawTileLine(ITile tile, int line, List<IPlayer> players, 
                               bool isLeftEdge, bool isRightEdge, bool isTopEdge, bool isBottomEdge)
    {
        ConsoleColor fgColor = ConsoleColor.White;

        // Set color based on property type
        if (tile.Asset != null && tile.Asset.ColorGroup > 0)
        {
            if (ColorGroupColors.TryGetValue(tile.Asset.ColorGroup, out var color))
            {
                fgColor = color;
            }
        }

        // Get players on this tile
        var playersOnTile = players.Where(p => p.CurrentTile == tile && p.State != PlayerState.Bankrupt).ToList();

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
                Console.ForegroundColor = fgColor;
                
                string displayName = GetShortName(tile.Name);
                string playerMarkers = GetPlayerMarkers(playersOnTile);
                string content = $"{displayName}{playerMarkers}";
                
                if (content.Length > TILE_WIDTH - 2)
                    content = content.Substring(0, TILE_WIDTH - 2);
                
                Console.Write(content.PadRight(TILE_WIDTH - 2));
                Console.ResetColor();
                Console.Write("│");
                break;

            case 2:
                // Bottom info (price or special)
                Console.Write("│");
                
                string info = "";
                if (tile.Asset != null)
                {
                    if (tile.Asset.Owner != null)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        info = $"[{tile.Asset.Owner.Symbol}]${tile.Asset.Value}";
                    }
                    else
                    {
                        info = $"${tile.Asset.Value}";
                    }
                }
                else
                {
                    info = GetSpecialTileInfo(tile);
                }

                if (info.Length > TILE_WIDTH - 2)
                    info = info.Substring(0, TILE_WIDTH - 2);
                
                Console.Write(info.PadRight(TILE_WIDTH - 2));
                Console.ResetColor();
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
            { "Avenue", "Ave" },
            { "Railroad", "RR" },
            { "Community Chest", "Comm" },
            { "Electric Company", "Elec Co" },
            { "Water Works", "Water" },
            { "Mediterranean", "Medit" },
            { "Connecticut", "Connect" },
            { "Pennsylvania", "Penn" },
            { "North Carolina", "N.Carol" },
            { "Free Parking", "FreePark" },
            { "Go To Jail", "ToJail" }
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
        return " " + string.Join("", players.Select(p => p.Symbol));
    }

    private string GetSpecialTileInfo(ITile tile)
    {
        return tile.EffectType switch
        {
            EffectType.GO => "+$200",
            EffectType.TAX => tile.Name.Contains("Luxury") ? "-$100" : "-$200",
            EffectType.COMMUNITY_CHEST => "CHEST",
            EffectType.CHANCE => "CHANCE",
            EffectType.GO_TO_JAIL => "JAIL!",
            EffectType.JAIL => "VISIT",
            EffectType.FREE_PARKING => "FREE",
            _ => ""
        };
    }

    public void ShowPlayerInfo(IPlayer player)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"═══ {player.Name} [{player.Symbol}] ═══");
        Console.ResetColor();
        
        Console.WriteLine($"  Money: ${player.Money.Balance}");
        Console.WriteLine($"  Position: {player.CurrentTile?.Name ?? "Unknown"} (Tile {player.RouteIndex})");
        Console.WriteLine($"  State: {player.State}");
        Console.WriteLine($"  Net Worth: ${player.GetNetWorth()}");
        
        if (player.Assets.Count > 0)
        {
            Console.WriteLine($"  Properties ({player.Assets.Count}):");
            foreach (var asset in player.Assets)
            {
                string status = asset.AssetsCondition == AssetsCondition.MORTGAGED ? " [M]" : "";
                string houses = asset.AmountHouse > 0 ? $" [{new string('H', Math.Min(asset.AmountHouse, 4))}{(asset.AmountHouse == 5 ? "!" : "")}]" : "";
                Console.WriteLine($"    - {asset.Name}{status}{houses}");
            }
        }
        
        if (player.HasGetOutOfJailCard)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  Has Get Out of Jail Free card!");
            Console.ResetColor();
        }
    }

    public void ShowAllPlayersInfo(List<IPlayer> players)
    {
        Console.WriteLine("\n╔═══════════════════════════════════════════╗");
        Console.WriteLine("║           PLAYERS STATUS                  ║");
        Console.WriteLine("╠═══════════════════════════════════════════╣");
        
        foreach (var player in players)
        {
            string status = player.State == PlayerState.Bankrupt ? " (BANKRUPT)" : "";
            string jail = player.State == PlayerState.InJail ? " [JAIL]" : "";
            Console.WriteLine($"║ {player.Symbol} {player.Name,-12} ${player.Money.Balance,-8} {player.Assets.Count} props{status}{jail}");
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
            Console.WriteLine("  DOUBLES!");
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
        string desc = card.Description;
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
        Console.Write("\nYour choice: ");
    }

    public int GetPlayerChoice(int maxOptions)
    {
        while (true)
        {
            if (int.TryParse(Console.ReadLine(), out int choice) && choice >= 1 && choice <= maxOptions)
            {
                return choice;
            }
            Console.Write($"Please enter a number between 1 and {maxOptions}: ");
        }
    }

    public string GetPlayerInput(string prompt)
    {
        Console.Write(prompt);
        return Console.ReadLine() ?? "";
    }

    public bool GetYesNo(string prompt)
    {
        Console.Write($"{prompt} (y/n): ");
        while (true)
        {
            string input = Console.ReadLine()?.ToLower() ?? "";
            if (input == "y" || input == "yes") return true;
            if (input == "n" || input == "no") return false;
            Console.Write("Please enter 'y' or 'n': ");
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
        
        Console.WriteLine($"║  Price: ${asset.Value,-28}  ║");
        Console.WriteLine($"║  Type: {asset.TypeAsset,-29}  ║");
        
        if (asset.TypeAsset == TypeAsset.REAL_ESTATE)
        {
            Console.WriteLine($"║  House Cost: ${asset.HouseCost,-23}  ║");
            Console.WriteLine("║  Rent:                                ║");
            string[] rentLabels = { "Base", "1 House", "2 Houses", "3 Houses", "4 Houses", "Hotel" };
            for (int i = 0; i < Math.Min(asset.Rent.Length, 6); i++)
            {
                Console.WriteLine($"║    {rentLabels[i],-10}: ${asset.Rent[i],-20}  ║");
            }
        }
        
        Console.WriteLine($"║  Mortgage Value: ${asset.GetMortgageValue(),-19}  ║");
        
        if (asset.Owner != null)
        {
            Console.WriteLine($"║  Owner: {asset.Owner.Name,-28}  ║");
            Console.WriteLine($"║  Houses: {asset.AmountHouse,-27}  ║");
            Console.WriteLine($"║  Status: {asset.AssetsCondition,-27}  ║");
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
        Console.WriteLine("║                    TRADE PROPOSAL                     ║");
        Console.WriteLine("╠══════════════════════════════════════════════════════╣");
        Console.ResetColor();
        
        Console.WriteLine($"║  {from.Name} offers:                                    ");
        foreach (var asset in offerFrom)
        {
            Console.WriteLine($"║    - {asset.Name}");
        }
        if (moneyFrom > 0)
            Console.WriteLine($"║    + ${moneyFrom}");
        
        Console.WriteLine("║                                                       ");
        Console.WriteLine($"║  In exchange for {to.Name}'s:                         ");
        foreach (var asset in offerTo)
        {
            Console.WriteLine($"║    - {asset.Name}");
        }
        if (moneyTo > 0)
            Console.WriteLine($"║    + ${moneyTo}");
        
        Console.WriteLine("╚══════════════════════════════════════════════════════╝");
    }

    public void ShowGameOver(IPlayer winner)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(@"
    ╔═══════════════════════════════════════════════════════╗
    ║                                                       ║
    ║      ██████╗  █████╗ ███╗   ███╗███████╗              ║
    ║     ██╔════╝ ██╔══██╗████╗ ████║██╔════╝              ║
    ║     ██║  ███╗███████║██╔████╔██║█████╗                ║
    ║     ██║   ██║██╔══██║██║╚██╔╝██║██╔══╝                ║
    ║     ╚██████╔╝██║  ██║██║ ╚═╝ ██║███████╗              ║
    ║      ╚═════╝ ╚═╝  ╚═╝╚═╝     ╚═╝╚══════╝              ║
    ║                                                       ║
    ║      ██████╗ ██╗   ██╗███████╗██████╗                 ║
    ║     ██╔═══██╗██║   ██║██╔════╝██╔══██╗                ║
    ║     ██║   ██║██║   ██║█████╗  ██████╔╝                ║
    ║     ██║   ██║╚██╗ ██╔╝██╔══╝  ██╔══██╗                ║
    ║     ╚██████╔╝ ╚████╔╝ ███████╗██║  ██║                ║
    ║      ╚═════╝   ╚═══╝  ╚══════╝╚═╝  ╚═╝                ║
    ║                                                       ║
    ╚═══════════════════════════════════════════════════════╝
        ");
        
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n    WINNER: {winner.Name}!");
        Console.WriteLine($"    Final Net Worth: ${winner.GetNetWorth()}");
        Console.WriteLine($"    Properties Owned: {winner.Assets.Count}");
        Console.ResetColor();
    }

    public void ShowWelcome()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(@"
    ╔═══════════════════════════════════════════════════════╗
    ║                                                       ║
    ║     ███╗   ███╗ ██████╗ ███╗   ██╗ ██████╗ ██████╗    ║
    ║     ████╗ ████║██╔═══██╗████╗  ██║██╔═══██╗██╔══██╗   ║
    ║     ██╔████╔██║██║   ██║██╔██╗ ██║██║   ██║██████╔╝   ║
    ║     ██║╚██╔╝██║██║   ██║██║╚██╗██║██║   ██║██╔═══╝    ║
    ║     ██║ ╚═╝ ██║╚██████╔╝██║ ╚████║╚██████╔╝██║        ║
    ║     ╚═╝     ╚═╝ ╚═════╝ ╚═╝  ╚═══╝ ╚═════╝ ╚═╝        ║
    ║                                                       ║
    ║              ██╗  ██╗   ██╗                            ║
    ║              ██║  ╚██╗ ██╔╝                            ║
    ║              ██║   ╚████╔╝                             ║
    ║              ██║    ╚██╔╝                              ║
    ║              ███████╗██║                               ║
    ║              ╚══════╝╚═╝                               ║
    ║                                                       ║
    ║           Console Edition - C# Version                ║
    ║                                                       ║
    ╚═══════════════════════════════════════════════════════╝
        ");
        Console.ResetColor();
        Console.WriteLine("\n    Press any key to start...");
        Console.ReadKey();
    }

    public void WaitForKeyPress()
    {
        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey();
    }
}
