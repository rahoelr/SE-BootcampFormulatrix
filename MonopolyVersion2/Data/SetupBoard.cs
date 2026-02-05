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

        // Bottom row (left to right): GO -> Jail
        var bottomRow = CreateBottomRow();
        for (int i = 0; i < bottomRow.Count; i++)
        {
            var tile = bottomRow[i];
            tile.Pos = new TilePos(i, 10);
            board.Grid[10, i] = tile;
            tile.PathIndex = pathIndex++;
            board.Path.Add(tile);
        }

        // Right column (bottom to top)
        var rightColumn = CreateRightColumn();
        for (int i = 0; i < rightColumn.Count; i++)
        {
            var tile = rightColumn[i];
            tile.Pos = new TilePos(10, 9 - i);
            board.Grid[9 - i, 10] = tile;
            tile.PathIndex = pathIndex++;
            board.Path.Add(tile);
        }

        // Top row (right to left)
        var topRow = CreateTopRow();
        for (int i = 0; i < topRow.Count; i++)
        {
            var tile = topRow[i];
            tile.Pos = new TilePos(10 - i, 0);
            board.Grid[0, 10 - i] = tile;
            tile.PathIndex = pathIndex++;
            board.Path.Add(tile);
        }

        // Left column (top to bottom)
        var leftColumn = CreateLeftColumn();
        for (int i = 0; i < leftColumn.Count; i++)
        {
            var tile = leftColumn[i];
            tile.Pos = new TilePos(0, 1 + i);
            board.Grid[1 + i, 0] = tile;
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
            CreateSpecialTile("MULAI", 'M', TilesType.Corner, EffectType.Go),
            
            // Aceh (Position 1)
            CreatePropertyTile("Aceh", 'A', 60, new[] { 2, 10, 30, 90, 160, 250 }, 50, 1),
            
            // Dana Umum (Position 2)
            CreateSpecialTile("Dana Umum", 'D', TilesType.Special, EffectType.CommunityChest),
            
            // Medan (Position 3)
            CreatePropertyTile("Medan", 'M', 60, new[] { 4, 20, 60, 180, 320, 450 }, 50, 1),
            
            // Pajak Penghasilan (Position 4)
            CreateSpecialTile("Pajak Penghasilan", 'P', TilesType.Special, EffectType.Tax),
            
            // Stasiun Gambir (Position 5)
            CreateRailroadTile("Stasiun Gambir", 'G'),
            
            // Palembang (Position 6)
            CreatePropertyTile("Palembang", 'P', 100, new[] { 6, 30, 90, 270, 400, 550 }, 50, 2),
            
            // Kesempatan (Position 7)
            CreateSpecialTile("Kesempatan", 'K', TilesType.Special, EffectType.Chance),
            
            // Padang (Position 8)
            CreatePropertyTile("Padang", 'P', 100, new[] { 6, 30, 90, 270, 400, 550 }, 50, 2),
            
            // Pekanbaru (Position 9)
            CreatePropertyTile("Pekanbaru", 'P', 120, new[] { 8, 40, 100, 300, 450, 600 }, 50, 2),
            
            // Penjara (Position 10)
            CreateSpecialTile("Penjara", 'J', TilesType.Corner, EffectType.GoToJail)
        };
    }

    private static List<Tile> CreateRightColumn()
    {
        return new List<Tile>
        {
            // Bandung (Position 11)
            CreatePropertyTile("Bandung", 'B', 140, new[] { 10, 50, 150, 450, 625, 750 }, 100, 3),
            
            // Perusahaan Listrik (Position 12)
            CreateUtilityTile("Perusahaan Listrik", 'L'),
            
            // Bogor (Position 13)
            CreatePropertyTile("Bogor", 'B', 140, new[] { 10, 50, 150, 450, 625, 750 }, 100, 3),
            
            // Tangerang (Position 14)
            CreatePropertyTile("Tangerang", 'T', 160, new[] { 12, 60, 180, 500, 700, 900 }, 100, 3),
            
            // Stasiun Pasar Senen (Position 15)
            CreateRailroadTile("Stasiun Pasar Senen", 'S'),
            
            // Bekasi (Position 16)
            CreatePropertyTile("Bekasi", 'B', 180, new[] { 14, 70, 200, 550, 750, 950 }, 100, 4),
            
            // Dana Umum (Position 17)
            CreateSpecialTile("Dana Umum", 'D', TilesType.Special, EffectType.CommunityChest),
            
            // Depok (Position 18)
            CreatePropertyTile("Depok", 'D', 180, new[] { 14, 70, 200, 550, 750, 950 }, 100, 4),
            
            // Jakarta (Position 19)
            CreatePropertyTile("Jakarta", 'J', 200, new[] { 16, 80, 220, 600, 800, 1000 }, 100, 4)
        };
    }

    private static List<Tile> CreateTopRow()
    {
        return new List<Tile>
        {
            // Parkir Gratis (Position 20)
            CreateSpecialTile("Parkir Gratis", 'P', TilesType.Corner, EffectType.FreeParking),
            
            // Semarang (Position 21)
            CreatePropertyTile("Semarang", 'S', 220, new[] { 18, 90, 250, 700, 875, 1050 }, 150, 5),
            
            // Kesempatan (Position 22)
            CreateSpecialTile("Kesempatan", 'K', TilesType.Special, EffectType.Chance),
            
            // Yogyakarta (Position 23)
            CreatePropertyTile("Yogyakarta", 'Y', 220, new[] { 18, 90, 250, 700, 875, 1050 }, 150, 5),
            
            // Solo (Position 24)
            CreatePropertyTile("Solo", 'S', 240, new[] { 20, 100, 300, 750, 925, 1100 }, 150, 5),
            
            // Stasiun Jatinegara (Position 25)
            CreateRailroadTile("Stasiun Jatinegara", 'J'),
            
            // Malang (Position 26)
            CreatePropertyTile("Malang", 'M', 260, new[] { 22, 110, 330, 800, 975, 1150 }, 150, 6),
            
            // Kediri (Position 27)
            CreatePropertyTile("Kediri", 'K', 260, new[] { 22, 110, 330, 800, 975, 1150 }, 150, 6),
            
            // Perusahaan Air (Position 28)
            CreateUtilityTile("Perusahaan Air", 'A'),
            
            // Surabaya (Position 29)
            CreatePropertyTile("Surabaya", 'S', 280, new[] { 24, 120, 360, 850, 1025, 1200 }, 150, 6),
            
            // Ke Penjara (Position 30)
            CreateSpecialTile("Ke Penjara", 'X', TilesType.Corner, EffectType.GoToJail)
        };
    }

    private static List<Tile> CreateLeftColumn()
    {
        return new List<Tile>
        {
            // Denpasar (Position 31)
            CreatePropertyTile("Denpasar", 'D', 300, new[] { 26, 130, 390, 900, 1100, 1275 }, 200, 7),
            
            // Mataram (Position 32)
            CreatePropertyTile("Mataram", 'M', 300, new[] { 26, 130, 390, 900, 1100, 1275 }, 200, 7),
            
            // Dana Umum (Position 33)
            CreateSpecialTile("Dana Umum", 'D', TilesType.Special, EffectType.CommunityChest),
            
            // Makassar (Position 34)
            CreatePropertyTile("Makassar", 'M', 320, new[] { 28, 150, 450, 1000, 1200, 1400 }, 200, 7),
            
            // Stasiun Manggarai (Position 35)
            CreateRailroadTile("Stasiun Manggarai", 'M'),
            
            // Kesempatan (Position 36)
            CreateSpecialTile("Kesempatan", 'K', TilesType.Special, EffectType.Chance),
            
            // Manado (Position 37)
            CreatePropertyTile("Manado", 'M', 350, new[] { 35, 175, 500, 1100, 1300, 1500 }, 200, 8),
            
            // Pajak Mewah (Position 38)
            CreateSpecialTile("Pajak Mewah", 'P', TilesType.Special, EffectType.Tax),
            
            // Jayapura (Position 39)
            CreatePropertyTile("Jayapura", 'J', 400, new[] { 50, 200, 600, 1400, 1700, 2000 }, 200, 8)
        };
    }

    private static Tile CreatePropertyTile(string name, char display, int price, int[] rent, int houseCost, int colorGroup)
    {
        var tile = new Tile(new TilePos(0, 0), name, display, TilesType.Property, EffectType.Nothing);
        tile.Value = price;
        tile.TypeAsset = TypeAsset.RealEstate;
        return tile;
    }

    private static Tile CreateUtilityTile(string name, char display)
    {
        var tile = new Tile(new TilePos(0, 0), name, display, TilesType.Utility, EffectType.Nothing);
        tile.Value = 150;
        tile.TypeAsset = TypeAsset.PublicService;
        return tile;
    }

    private static Tile CreateRailroadTile(string name, char display)
    {
        var tile = new Tile(new TilePos(0, 0), name, display, TilesType.Railroad, EffectType.Nothing);
        tile.Value = 200;
        tile.TypeAsset = TypeAsset.Railroad;
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
            new Card("Maju ke MULAI", "Maju ke MULAI (Terima $200)", CardEffect.Move, 0),
            new Card("Maju ke Solo", "Maju ke Solo", CardEffect.Move, 24),
            new Card("Maju ke Bandung", "Maju ke Bandung", CardEffect.Move, 11),
            new Card("Dividen Bank", "Bank membayar dividen $50", CardEffect.ReceiveMoney, 50),
            new Card("Bebas Penjara", "Kartu bebas dari penjara", CardEffect.GetOutJail, 0),
            new Card("Mundur 3 Langkah", "Mundur 3 langkah", CardEffect.Move, -3),
            new Card("Masuk Penjara", "Langsung masuk penjara", CardEffect.GoToJail, 0),
            new Card("Perbaikan Umum", "Lakukan perbaikan umum. Bayar $25 per rumah", CardEffect.PayMoney, 25),
            new Card("Denda Kecepatan", "Denda kecepatan $15", CardEffect.PayMoney, 15),
            new Card("Maju ke Stasiun Gambir", "Perjalanan ke Stasiun Gambir", CardEffect.Move, 5),
            new Card("Maju ke Jayapura", "Maju ke Jayapura", CardEffect.Move, 39),
            new Card("Ketua Dewan", "Anda terpilih sebagai Ketua Dewan. Bayar setiap pemain $50", CardEffect.PayMoney, 50),
            new Card("Pinjaman Bangunan", "Pinjaman bangunan Anda jatuh tempo. Terima $150", CardEffect.ReceiveMoney, 150),
            new Card("Hadiah Teka-teki", "Anda memenangkan kompetisi teka-teki silang. Terima $100", CardEffect.ReceiveMoney, 100)
        };

        return new Decks(cards);
    }

    public static Decks CreateCommunityChestDeck()
    {
        var cards = new List<ICard>
        {
            new Card("Maju ke MULAI", "Maju ke MULAI (Terima $200)", CardEffect.Move, 0),
            new Card("Kesalahan Bank", "Kesalahan bank menguntungkan Anda. Terima $200", CardEffect.ReceiveMoney, 200),
            new Card("Tagihan Dokter", "Tagihan dokter. Bayar $50", CardEffect.PayMoney, 50),
            new Card("Penjualan Saham", "Dari penjualan saham Anda mendapat $50", CardEffect.ReceiveMoney, 50),
            new Card("Bebas Penjara", "Kartu bebas dari penjara", CardEffect.GetOutJail, 0),
            new Card("Masuk Penjara", "Langsung masuk penjara. Jangan lewati MULAI", CardEffect.GoToJail, 0),
            new Card("Hadiah Konser", "Anda menerima royalti dari konser. Terima $100", CardEffect.ReceiveMoney, 100),
            new Card("Hadiah Ulang Tahun", "Hari ulang tahun Anda. Terima $10 dari setiap pemain", CardEffect.ReceiveMoney, 10),
            new Card("Pengembalian Pajak", "Pengembalian pajak penghasilan. Terima $20", CardEffect.ReceiveMoney, 20),
            new Card("Premi Asuransi", "Premi asuransi jiwa jatuh tempo. Bayar $50", CardEffect.PayMoney, 50),
            new Card("Biaya Rumah Sakit", "Bayar biaya rumah sakit $100", CardEffect.PayMoney, 100),
            new Card("Biaya Sekolah", "Bayar biaya sekolah $50", CardEffect.PayMoney, 50),
            new Card("Dana Konsultasi", "Terima dana konsultasi $25", CardEffect.ReceiveMoney, 25),
            new Card("Perbaikan Jalan", "Anda dinilai untuk perbaikan jalan. Bayar $40 per rumah, $115 per hotel", CardEffect.PayMoney, 40),
            new Card("Warisan", "Anda mewarisi $100", CardEffect.ReceiveMoney, 100),
            new Card("Kompetisi Kecantikan", "Anda juara kedua dalam kompetisi kecantikan. Terima $10", CardEffect.ReceiveMoney, 10)
        };

        return new Decks(cards);
    }
}