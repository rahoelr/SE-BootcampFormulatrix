using MonopolyApp.Enums;
using MonopolyApp.Interfaces;
using MonopolyApp.Models;
using MonopolyApp.Structs;

namespace MonopolyApp.Data;

public static class SetupBoard
{
    public static Board CreateStandardBoard()
    {
        var board = new Board(11, 11);
        int pathIndex = 0;

        // Create tiles for the path (going clockwise from bottom-left GO)
        // Bottom row (left to right): GO -> Jail
        //0,10 -> 10,10 kiri ke kanan cuy
        var bottomRow = CreateBottomRow();
        for (int i = 0; i < bottomRow.Count; i++)
        {
            var tile = bottomRow[i];
            board.SetTile(i, 10, tile);
            tile.PathIndex = pathIndex++;
            board.Path.Add(tile);
        }

        // 10,9 -> 10,1 (bawah ke atas)
        // Right column (bottom to top, excluding corners)
        var rightColumn = CreateRightColumn();
        for (int i = 0; i < rightColumn.Count; i++)
        {
            var tile = rightColumn[i];
            board.SetTile(10, 9 - i, tile);
            tile.PathIndex = pathIndex++;
            board.Path.Add(tile);
        }

        // 10,0 -> 0,0 kanan ke kiri
        // Top row (right to left): Free Parking -> Go To Jail
        var topRow = CreateTopRow();
        for (int i = 0; i < topRow.Count; i++)
        {
            var tile = topRow[i];
            board.SetTile(10 - i, 0, tile);
            tile.PathIndex = pathIndex++;
            board.Path.Add(tile);
        }

        // dari 0,1 -> 0,9 atas ke bawah
        // Left column (top to bottom, excluding corners)
        var leftColumn = CreateLeftColumn();
        for (int i = 0; i < leftColumn.Count; i++)
        {
            var tile = leftColumn[i];
            board.SetTile(0, 1 + i, tile);
            tile.PathIndex = pathIndex++;
            board.Path.Add(tile);
        }

        return board;
    }

    private static List<Tile> CreateBottomRow()
    {
        return new List<Tile>
        {
            // GO (Position 0)
            CreateSpecialTile("GO", 'G', TilesType.CORNER, EffectType.g),
            
            // Mediterranean Avenue (Position 1)
            CreatePropertyTile("Mediterranean Ave", 'M', 60, new[] { 2, 10, 30, 90, 160, 250 }, 50, 1),
            
            // Community Chest (Position 2)
            CreateSpecialTile("Community Chest", 'C', TilesType.SPECIAL, EffectType.COMMUNITY_CHEST),
            
            // Baltic Avenue (Position 3)
            CreatePropertyTile("Baltic Ave", 'B', 60, new[] { 4, 20, 60, 180, 320, 450 }, 50, 1),
            
            // Income Tax (Position 4)
            CreateSpecialTile("Income Tax", 'T', TilesType.SPECIAL, EffectType.TAX),
            
            // Reading Railroad (Position 5)
            CreateRailroadTile("Reading RR", 'R'),
            
            // Oriental Avenue (Position 6)
            CreatePropertyTile("Oriental Ave", 'O', 100, new[] { 6, 30, 90, 270, 400, 550 }, 50, 2),
            
            // Chance (Position 7)
            CreateSpecialTile("Chance", '?', TilesType.SPECIAL, EffectType.CHANCE),
            
            // Vermont Avenue (Position 8)
            CreatePropertyTile("Vermont Ave", 'V', 100, new[] { 6, 30, 90, 270, 400, 550 }, 50, 2),
            
            // Connecticut Avenue (Position 9)
            CreatePropertyTile("Connecticut Ave", 'C', 120, new[] { 8, 40, 100, 300, 450, 600 }, 50, 2),
            
            // Jail / Just Visiting (Position 10)
            CreateSpecialTile("Jail", 'J', TilesType.CORNER, EffectType.JAIL)
        };
    }

    private static List<Tile> CreateRightColumn()
    {
        return new List<Tile>
        {
            // St. Charles Place (Position 11)
            CreatePropertyTile("St. Charles Pl", 'S', 140, new[] { 10, 50, 150, 450, 625, 750 }, 100, 3),
            
            // Electric Company (Position 12)
            CreateUtilityTile("Electric Co", 'E'),
            
            // States Avenue (Position 13)
            CreatePropertyTile("States Ave", 'S', 140, new[] { 10, 50, 150, 450, 625, 750 }, 100, 3),
            
            // Virginia Avenue (Position 14)
            CreatePropertyTile("Virginia Ave", 'V', 160, new[] { 12, 60, 180, 500, 700, 900 }, 100, 3),
            
            // Pennsylvania Railroad (Position 15)
            CreateRailroadTile("Pennsylvania RR", 'P'),
            
            // St. James Place (Position 16)
            CreatePropertyTile("St. James Pl", 'S', 180, new[] { 14, 70, 200, 550, 750, 950 }, 100, 4),
            
            // Community Chest (Position 17)
            CreateSpecialTile("Community Chest", 'C', TilesType.SPECIAL, EffectType.COMMUNITY_CHEST),
            
            // Tennessee Avenue (Position 18)
            CreatePropertyTile("Tennessee Ave", 'T', 180, new[] { 14, 70, 200, 550, 750, 950 }, 100, 4),
            
            // New York Avenue (Position 19)
            CreatePropertyTile("New York Ave", 'N', 200, new[] { 16, 80, 220, 600, 800, 1000 }, 100, 4)
        };
    }

    private static List<Tile> CreateTopRow()
    {
        return new List<Tile>
        {
            // Free Parking (Position 20)
            CreateSpecialTile("Free Parking", 'F', TilesType.CORNER, EffectType.FREE_PARKING),
            
            // Kentucky Avenue (Position 21)
            CreatePropertyTile("Kentucky Ave", 'K', 220, new[] { 18, 90, 250, 700, 875, 1050 }, 150, 5),
            
            // Chance (Position 22)
            CreateSpecialTile("Chance", '?', TilesType.SPECIAL, EffectType.CHANCE),
            
            // Indiana Avenue (Position 23)
            CreatePropertyTile("Indiana Ave", 'I', 220, new[] { 18, 90, 250, 700, 875, 1050 }, 150, 5),
            
            // Illinois Avenue (Position 24)
            CreatePropertyTile("Illinois Ave", 'I', 240, new[] { 20, 100, 300, 750, 925, 1100 }, 150, 5),
            
            // B&O Railroad (Position 25)
            CreateRailroadTile("B&O RR", 'B'),
            
            // Atlantic Avenue (Position 26)
            CreatePropertyTile("Atlantic Ave", 'A', 260, new[] { 22, 110, 330, 800, 975, 1150 }, 150, 6),
            
            // Ventnor Avenue (Position 27)
            CreatePropertyTile("Ventnor Ave", 'V', 260, new[] { 22, 110, 330, 800, 975, 1150 }, 150, 6),
            
            // Water Works (Position 28)
            CreateUtilityTile("Water Works", 'W'),
            
            // Marvin Gardens (Position 29)
            CreatePropertyTile("Marvin Gardens", 'M', 280, new[] { 24, 120, 360, 850, 1025, 1200 }, 150, 6),
            
            // Go To Jail (Position 30)
            CreateSpecialTile("Go To Jail", 'X', TilesType.CORNER, EffectType.GO_TO_JAIL)
        };
    }

    private static List<Tile> CreateLeftColumn()
    {
        return new List<Tile>
        {
            // Pacific Avenue (Position 31)
            CreatePropertyTile("Pacific Ave", 'P', 300, new[] { 26, 130, 390, 900, 1100, 1275 }, 200, 7),
            
            // North Carolina Avenue (Position 32)
            CreatePropertyTile("N. Carolina Ave", 'N', 300, new[] { 26, 130, 390, 900, 1100, 1275 }, 200, 7),
            
            // Community Chest (Position 33)
            CreateSpecialTile("Community Chest", 'C', TilesType.SPECIAL, EffectType.COMMUNITY_CHEST),
            
            // Pennsylvania Avenue (Position 34)
            CreatePropertyTile("Pennsylvania Ave", 'P', 320, new[] { 28, 150, 450, 1000, 1200, 1400 }, 200, 7),
            
            // Short Line Railroad (Position 35)
            CreateRailroadTile("Short Line RR", 'S'),
            
            // Chance (Position 36)
            CreateSpecialTile("Chance", '?', TilesType.SPECIAL, EffectType.CHANCE),
            
            // Park Place (Position 37)
            CreatePropertyTile("Park Place", 'P', 350, new[] { 35, 175, 500, 1100, 1300, 1500 }, 200, 8),
            
            // Luxury Tax (Position 38)
            CreateSpecialTile("Luxury Tax", 'L', TilesType.SPECIAL, EffectType.TAX),
            
            // Boardwalk (Position 39)
            CreatePropertyTile("Boardwalk", 'B', 400, new[] { 50, 200, 600, 1400, 1700, 2000 }, 200, 8)
        };
    }
     private static Tile CreatePropertyTile(string name, char display, int price, int[] rent, int houseCost, int colorGroup)
    {
        var tile = new Tile(new TilePos(0, 0), name, display, TilesType.PROPERTY, EffectType.NOTHING);
        tile.Asset = new Asset(name, TypeAsset.RealEstate, price, rent, houseCost, colorGroup);
        return tile;
    }

    private static Tile CreateRailroadTile(string name, char display)
    {
        var tile = new Tile(new TilePos(0, 0), name, display, TilesType.RAILROAD, EffectType.NOTHING);
        // Railroad rent: 25, 50, 100, 200 based on number owned
        tile.Asset = new Asset(name, TypeAsset., 200, new[] { 25, 50, 100, 200 }, 0, 0);
        return tile;
    }

    private static Tile CreateUtilityTile(string name, char display)
    {
        var tile = new Tile(new TilePos(0, 0), name, display, TilesType.UTILITY, EffectType.Nothing);
        // Utility rent: 4x dice if 1 owned, 10x dice if both owned
        tile. = new Asset(name, TypeAsset.PublicService, 150, new[] { 4, 10 }, 0, 0);
        return tile;
    }

    private static Tile CreateSpecialTile(string name, char display, TilesType type, EffectType effectType)
    {
        return new Tile(new TilePos(0, 0), name, display, type, effectType);
    }

    public static Decks CreateChanceDeck()
    {
        var cards = new List<ICard>
        {
            new Card("Advance to GO", "Advance to GO (Collect $200)", CardEffect.Move, 0),
            new Card("Advance to Illinois", "Advance to Illinois Avenue", CardEffect.Move, 24),
            new Card("Advance to St. Charles", "Advance to St. Charles Place", CardEffect.Move, 11),
            new Card("Bank Dividend", "Bank pays you dividend of $50", CardEffect.ReceiveMoney, 50),
            new Card("Get Out of Jail Free", "Get out of Jail free card", CardEffect.GetOutJail, 0),
            new Card("Go Back 3 Spaces", "Go back 3 spaces", CardEffect.Move, -3),
            new Card("Go to Jail", "Go directly to jail", CardEffect.GoToJail, 0),
            new Card("General Repairs", "Make general repairs. Pay $25 per house", CardEffect.PayMoney, 25),
            new Card("Speeding Fine", "Speeding fine $15", CardEffect.PayMoney, 15),
            new Card("Advance to Reading RR", "Take a trip to Reading Railroad", CardEffect.Move, 5),
            new Card("Advance to Boardwalk", "Advance to Boardwalk", CardEffect.Move, 39),
            new Card("Chairman of the Board", "You have been elected Chairman of the Board. Pay each player $50", CardEffect.PAY_MONEY, 50),
            new Card("Building Loan", "Your building loan matures. Collect $150", CardEffect.ReceiveMoney, 150),
            new Card("Crossword Prize", "You have won a crossword competition. Collect $100", CardEffect.ReceiveMoney, 100)
        };

        return new Decks(cards);
    }
}