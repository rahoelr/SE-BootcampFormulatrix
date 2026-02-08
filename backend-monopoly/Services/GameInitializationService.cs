using MonopolyBackend.Models;
using MonopolyBackend.Interfaces;
using MonopolyBackend.Enums;
using MonopolyBackend.Structs;
using MonopolyBackend.DTOs.Responses;

namespace MonopolyBackend.Services
{
    public static class GameInitializationService
    {
        public static GameService CreateGame(List<string> playerNames)
        {
            var board = CreateSimpleBoard();
            var players = CreatePlayers(playerNames);
            var dices = new List<IDice> { new Dice(6), new Dice(6) };
            var tileAssets = CreateTileAssetMapping(board);
            var communityChestDeck = CreateCommunityChestDeck();
            var chanceDeck = CreateChanceDeck();

            var gameService = new GameService(
                board,
                players,
                dices,
                communityChestDeck,
                chanceDeck,
                tileAssets
            );

            gameService.StartGame();
            return gameService;
        }

        public static BoardResponse GetBoardConfiguration()
        {
            var board = CreateSimpleBoard();
            var tileAssets = CreateTileAssetMapping(board);

            var tiles = board.Path.Select(tile => new TileResponse
            {
                Position = tile.PathIndex ?? 0,
                Name = tile.Name,
                Type = tile.TilesType.ToString(),
                Effect = tile.EffectType.ToString(),
                Price = tileAssets.TryGetValue(tile, out var asset) ? asset?.Value : null,
                AssetType = tileAssets.TryGetValue(tile, out var assetVal) ? assetVal?.TypeAsset.ToString() : null
            }).ToList();

            return new BoardResponse
            {
                Tiles = tiles,
                TotalTiles = tiles.Count
            };
        }

        private static List<IPlayer> CreatePlayers(List<string> playerNames)
        {
            return playerNames
                .Select(name => (IPlayer)new Player(name, new Money(1500)))
                .ToList();
        }

        private static IBoard CreateSimpleBoard()
        {
            var board = new Board(11, 11);
            
            var tiles = new List<ITile>
            {
                CreateSimpleTile(0, "GO", TilesType.Corner, EffectType.Go),
                CreateSimpleTile(1, "Medan", TilesType.Property, EffectType.Nothing),
                CreateSimpleTile(2, "Community Chest", TilesType.Special, EffectType.CommunityChest),
                CreateSimpleTile(3, "Palembang", TilesType.Property, EffectType.Nothing),
                CreateSimpleTile(4, "Income Tax", TilesType.Special, EffectType.Tax),
                CreateSimpleTile(5, "Stasiun Gambir", TilesType.Railroad, EffectType.Nothing),
                CreateSimpleTile(6, "Semarang", TilesType.Property, EffectType.Nothing),
                CreateSimpleTile(7, "Chance", TilesType.Special, EffectType.Chance),
                CreateSimpleTile(8, "Surabaya", TilesType.Property, EffectType.Nothing),
                CreateSimpleTile(9, "Makassar", TilesType.Property, EffectType.Nothing),
                CreateSimpleTile(10, "Jail / Just Visiting", TilesType.Corner, EffectType.Nothing),
                CreateSimpleTile(11, "Bandung", TilesType.Property, EffectType.Nothing),
                CreateSimpleTile(12, "PLN", TilesType.Utility, EffectType.Nothing),
                CreateSimpleTile(13, "Yogyakarta", TilesType.Property, EffectType.Nothing),
                CreateSimpleTile(14, "Solo", TilesType.Property, EffectType.Nothing),
                CreateSimpleTile(15, "Stasiun Pasar Senen", TilesType.Railroad, EffectType.Nothing),
                CreateSimpleTile(16, "Denpasar", TilesType.Property, EffectType.Nothing),
                CreateSimpleTile(17, "Community Chest", TilesType.Special, EffectType.CommunityChest),
                CreateSimpleTile(18, "Malang", TilesType.Property, EffectType.Nothing),
                CreateSimpleTile(19, "Balikpapan", TilesType.Property, EffectType.Nothing),
                CreateSimpleTile(20, "Free Parking", TilesType.Corner, EffectType.FreeParking),
                CreateSimpleTile(21, "Manado", TilesType.Property, EffectType.Nothing),
                CreateSimpleTile(22, "Chance", TilesType.Special, EffectType.Chance),
                CreateSimpleTile(23, "Pontianak", TilesType.Property, EffectType.Nothing),
                CreateSimpleTile(24, "Batam", TilesType.Property, EffectType.Nothing),
                CreateSimpleTile(25, "Stasiun Manggarai", TilesType.Railroad, EffectType.Nothing),
                CreateSimpleTile(26, "Depok", TilesType.Property, EffectType.Nothing),
                CreateSimpleTile(27, "Tangerang", TilesType.Property, EffectType.Nothing),
                CreateSimpleTile(28, "PDAM", TilesType.Utility, EffectType.Nothing),
                CreateSimpleTile(29, "Bekasi", TilesType.Property, EffectType.Nothing),
                CreateSimpleTile(30, "Go To Jail", TilesType.Corner, EffectType.GoToJail),
                CreateSimpleTile(31, "Bogor", TilesType.Property, EffectType.Nothing),
                CreateSimpleTile(32, "Jakarta Selatan", TilesType.Property, EffectType.Nothing),
                CreateSimpleTile(33, "Community Chest", TilesType.Special, EffectType.CommunityChest),
                CreateSimpleTile(34, "Jakarta Pusat", TilesType.Property, EffectType.Nothing),
                CreateSimpleTile(35, "Stasiun Tanah Abang", TilesType.Railroad, EffectType.Nothing),
                CreateSimpleTile(36, "Chance", TilesType.Special, EffectType.Chance),
                CreateSimpleTile(37, "Jakarta Utara", TilesType.Property, EffectType.Nothing),
                CreateSimpleTile(38, "Luxury Tax", TilesType.Special, EffectType.Tax),
                CreateSimpleTile(39, "Jakarta Barat", TilesType.Property, EffectType.Nothing),
            };

            board.Path.AddRange(tiles);
            return board;
        }

        private static ITile CreateSimpleTile(int pathIndex, string name, TilesType type, EffectType effect)
        {
            return new Tile(
                pos: new TilePos(0, 0),
                name: name,
                display: ' ',
                type: type,
                effectType: effect,
                pathIndex: pathIndex
            );
        }

        private static Dictionary<ITile, IAsset?> CreateTileAssetMapping(IBoard board)
        {
            var mapping = new Dictionary<ITile, IAsset?>();

            // Brown
            mapping[board.Path[1]] = new Asset("Medan", TypeAsset.RealEstate, 60);
            mapping[board.Path[3]] = new Asset("Palembang", TypeAsset.RealEstate, 60);

            // Light Blue
            mapping[board.Path[6]] = new Asset("Semarang", TypeAsset.RealEstate, 100);
            mapping[board.Path[8]] = new Asset("Surabaya", TypeAsset.RealEstate, 100);
            mapping[board.Path[9]] = new Asset("Makassar", TypeAsset.RealEstate, 120);

            // Pink
            mapping[board.Path[11]] = new Asset("Bandung", TypeAsset.RealEstate, 140);
            mapping[board.Path[13]] = new Asset("Yogyakarta", TypeAsset.RealEstate, 140);
            mapping[board.Path[14]] = new Asset("Solo", TypeAsset.RealEstate, 160);

            // Orange
            mapping[board.Path[16]] = new Asset("Denpasar", TypeAsset.RealEstate, 180);
            mapping[board.Path[18]] = new Asset("Malang", TypeAsset.RealEstate, 180);
            mapping[board.Path[19]] = new Asset("Balikpapan", TypeAsset.RealEstate, 200);

            // Red
            mapping[board.Path[21]] = new Asset("Manado", TypeAsset.RealEstate, 220);
            mapping[board.Path[23]] = new Asset("Pontianak", TypeAsset.RealEstate, 220);
            mapping[board.Path[24]] = new Asset("Batam", TypeAsset.RealEstate, 240);

            // Yellow
            mapping[board.Path[26]] = new Asset("Depok", TypeAsset.RealEstate, 260);
            mapping[board.Path[27]] = new Asset("Tangerang", TypeAsset.RealEstate, 260);
            mapping[board.Path[29]] = new Asset("Bekasi", TypeAsset.RealEstate, 280);

            // Green
            mapping[board.Path[31]] = new Asset("Bogor", TypeAsset.RealEstate, 300);
            mapping[board.Path[32]] = new Asset("Jakarta Selatan", TypeAsset.RealEstate, 300);
            mapping[board.Path[34]] = new Asset("Jakarta Pusat", TypeAsset.RealEstate, 320);

            // Dark Blue
            mapping[board.Path[37]] = new Asset("Jakarta Utara", TypeAsset.RealEstate, 350);
            mapping[board.Path[39]] = new Asset("Jakarta Barat", TypeAsset.RealEstate, 400);

            // Railroads
            mapping[board.Path[5]] = new Asset("Stasiun Gambir", TypeAsset.Railroad, 200);
            mapping[board.Path[15]] = new Asset("Stasiun Pasar Senen", TypeAsset.Railroad, 200);
            mapping[board.Path[25]] = new Asset("Stasiun Manggarai", TypeAsset.Railroad, 200);
            mapping[board.Path[35]] = new Asset("Stasiun Tanah Abang", TypeAsset.Railroad, 200);

            // Utilities
            mapping[board.Path[12]] = new Asset("PLN", TypeAsset.PublicService, 150);
            mapping[board.Path[28]] = new Asset("PDAM", TypeAsset.PublicService, 150);

            return mapping;
        }

        private static IDecks CreateCommunityChestDeck()
        {
            var cards = new List<ICard>
            {
                new Card("Advance to Go", "Advance to Go (Collect $200)", CardEffect.Move, 0),
                new Card("Bank Error", "Bank error in your favor - Collect $200", CardEffect.ReceiveMoney, 200),
                new Card("Doctor Fee", "Doctor's fee - Pay $50", CardEffect.PayMoney, 50),
                new Card("Stock Sale", "From sale of stock you get $50", CardEffect.ReceiveMoney, 50),
                new Card("Get Out of Jail Free", "Get Out of Jail Free", CardEffect.GetOutJail, 0),
                new Card("Go to Jail", "Go to Jail - Go directly to jail", CardEffect.GoToJail, 0),
                new Card("Holiday Fund", "Holiday fund matures - Receive $100", CardEffect.ReceiveMoney, 100),
                new Card("Income Tax Refund", "Income tax refund - Collect $20", CardEffect.ReceiveMoney, 20),
                new Card("Birthday", "It is your birthday - Collect $10", CardEffect.ReceiveMoney, 10),
                new Card("Life Insurance", "Life insurance matures - Collect $100", CardEffect.ReceiveMoney, 100),
                new Card("Hospital Fees", "Pay hospital fees of $100", CardEffect.PayMoney, 100),
                new Card("School Fees", "Pay school fees of $50", CardEffect.PayMoney, 50),
                new Card("Consultancy Fee", "Receive $25 consultancy fee", CardEffect.ReceiveMoney, 25),
                new Card("Street Repairs", "You are assessed for street repairs - Pay $40", CardEffect.PayMoney, 40),
                new Card("Beauty Contest", "You have won second prize in a beauty contest - Collect $10", CardEffect.ReceiveMoney, 10),
                new Card("Inheritance", "You inherit $100", CardEffect.ReceiveMoney, 100)
            };

            return new Decks(cards);
        }

        private static IDecks CreateChanceDeck()
        {
            var cards = new List<ICard>
            {
                new Card("Advance to Go", "Advance to Go (Collect $200)", CardEffect.Move, 0),
                new Card("Advance to Illinois", "Advance to Illinois Avenue", CardEffect.Move, 24),
                new Card("Advance to St. Charles", "Advance to St. Charles Place", CardEffect.Move, 11),
                new Card("Advance to Nearest Utility", "Advance to nearest Utility", CardEffect.Move, -1),
                new Card("Advance to Nearest Railroad", "Advance to nearest Railroad", CardEffect.Move, -1),
                new Card("Bank Dividend", "Bank pays you dividend of $50", CardEffect.ReceiveMoney, 50),
                new Card("Get Out of Jail Free", "Get Out of Jail Free", CardEffect.GetOutJail, 0),
                new Card("Go Back 3 Spaces", "Go back 3 spaces", CardEffect.Move, -3),
                new Card("Go to Jail", "Go to Jail - Go directly to Jail", CardEffect.GoToJail, 0),
                new Card("General Repairs", "Make general repairs - Pay $25 per house, $100 per hotel", CardEffect.PayMoney, 25),
                new Card("Speeding Fine", "Speeding fine - Pay $15", CardEffect.PayMoney, 15),
                new Card("Reading Railroad", "Take a trip to Reading Railroad", CardEffect.Move, 5),
                new Card("Boardwalk", "Take a walk on the Boardwalk - Advance to Boardwalk", CardEffect.Move, 39),
                new Card("Chairman of Board", "You have been elected Chairman of the Board - Pay each player $50", CardEffect.PayMoney, 50),
                new Card("Building Loan", "Your building loan matures - Receive $150", CardEffect.ReceiveMoney, 150),
                new Card("Crossword Competition", "You have won a crossword competition - Collect $100", CardEffect.ReceiveMoney, 100)
            };

            return new Decks(cards);
        }
    }
}
