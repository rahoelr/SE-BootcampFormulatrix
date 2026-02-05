# 📚 Dokumentasi Lengkap Console Monopoly C#

## Daftar Isi
1. [Pendahuluan](#pendahuluan)
2. [Arsitektur Aplikasi](#arsitektur-aplikasi)
3. [Struktur Proyek](#struktur-proyek)
4. [Komponen Utama](#komponen-utama)
5. [Game Flow (Alur Permainan)](#game-flow-alur-permainan)
6. [Dokumentasi Kode per File](#dokumentasi-kode-per-file)
7. [Mekanik Permainan](#mekanik-permainan)
8. [Skenario Penggunaan](#skenario-penggunaan)
9. [Cara Menjalankan](#cara-menjalankan)

---

## Pendahuluan

Console Monopoly adalah implementasi permainan Monopoly klasik yang dibuat menggunakan C# dengan arsitektur MVC (Model-View-Controller). Aplikasi ini berjalan di console/terminal dan mendukung 2-4 pemain.

### Fitur Utama
- ✅ Permainan 2-4 pemain
- ✅ Papan permainan dengan 40 tile (properti kota-kota Indonesia)
- ✅ Sistem jual-beli properti
- ✅ Sistem pembangunan rumah dan hotel
- ✅ Kartu Kesempatan dan Dana Umum
- ✅ Sistem penjara dengan berbagai opsi keluar
- ✅ Sistem mortgage properti
- ✅ Perdagangan antar pemain
- ✅ Visualisasi papan di console

---

## Arsitektur Aplikasi

Aplikasi ini menggunakan pola arsitektur **MVC (Model-View-Controller)**:

```
┌─────────────────────────────────────────────────────────────────┐
│                        PROGRAM.CS                               │
│                    (Entry Point / Main)                         │
└─────────────────────────────────────────────────────────────────┘
                               │
                               ▼
┌─────────────────────────────────────────────────────────────────┐
│                                                                 │
│   ┌─────────────┐    ┌────────────────┐    ┌──────────────┐   │
│   │   MODELS    │◄───│   CONTROLLER   │───►│    VIEW      │   │
│   │             │    │                │    │              │   │
│   │ - Player    │    │ GameController │    │ ConsoleView  │   │
│   │ - Asset     │    │                │    │              │   │
│   │ - Board     │    │ - PlayTurn()   │    │ - DrawBoard  │   │
│   │ - Tile      │    │ - RollDices()  │    │ - ShowMenu   │   │
│   │ - Card      │    │ - OnLand()     │    │ - GetInput   │   │
│   │ - Money     │    │ - Trade()      │    │              │   │
│   │ - Dice      │    │                │    │              │   │
│   └─────────────┘    └────────────────┘    └──────────────┘   │
│         ▲                    │                                  │
│         │                    ▼                                  │
│   ┌─────────────┐    ┌────────────────┐                        │
│   │ INTERFACES  │    │     DATA       │                        │
│   │             │    │                │                        │
│   │ - IPlayer   │    │ SetupBoard.cs  │                        │
│   │ - IAsset    │    │ (Board Config) │                        │
│   │ - IBoard    │    │                │                        │
│   │ - ITile     │    │                │                        │
│   │ - IView     │    │                │                        │
│   │ - ICard     │    │                │                        │
│   │ - IMoney    │    │                │                        │
│   │ - IDice     │    │                │                        │
│   └─────────────┘    └────────────────┘                        │
│                                                                 │
│   ┌─────────────┐    ┌────────────────┐                        │
│   │   ENUMS     │    │    STRUCTS     │                        │
│   │             │    │                │                        │
│   │ - TilesType │    │ - TilePos      │                        │
│   │ - TypeAsset │    │                │                        │
│   │ - PlayerSt. │    │                │                        │
│   │ - CardEff.  │    │                │                        │
│   │ - EffectTy. │    │                │                        │
│   │ - AssetCond │    │                │                        │
│   └─────────────┘    └────────────────┘                        │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### Penjelasan Arsitektur:

| Layer | Tanggung Jawab |
|-------|----------------|
| **Model** | Menyimpan data dan state permainan (Player, Asset, Board, dll.) |
| **View** | Menampilkan antarmuka pengguna di console |
| **Controller** | Mengontrol logika permainan dan alur game |
| **Interfaces** | Kontrak yang memastikan konsistensi antar komponen |
| **Enums** | Konstanta untuk tipe-tipe data tertentu |
| **Data** | Konfigurasi papan dan kartu permainan |

---

## Struktur Proyek

```
MonopolyVersion2/
├── 📁 ConsoleView/
│   └── View.cs                  # Implementasi tampilan console
├── 📁 Controllers/
│   └── GameController.cs        # Logika utama permainan
├── 📁 Data/
│   └── SetupBoard.cs            # Konfigurasi papan & kartu
├── 📁 Enums/
│   ├── AssetCondition.cs        # Normal, Mortgage
│   ├── CardEffect.cs            # ReceiveMoney, PayMoney, dll.
│   ├── EffectType.cs            # Go, Tax, GoToJail, dll.
│   ├── PlayerState.cs           # Normal, InJail, Bankrupt
│   ├── TilesType.cs             # Corner, Property, Railroad, dll.
│   └── TypeAsset.cs             # RealEstate, PublicService, Railroad
├── 📁 Interfaces/
│   ├── IAsset.cs                # Interface untuk properti
│   ├── IBoard.cs                # Interface untuk papan
│   ├── ICard.cs                 # Interface untuk kartu
│   ├── IDecks.cs                # Interface untuk deck kartu
│   ├── IDice.cs                 # Interface untuk dadu
│   ├── IMoney.cs                # Interface untuk uang
│   ├── IPlayer.cs               # Interface untuk pemain
│   ├── ITile.cs                 # Interface untuk tile
│   └── IView.cs                 # Interface untuk tampilan
├── 📁 Models/
│   ├── Asset.cs                 # Model properti
│   ├── Board.cs                 # Model papan
│   ├── Card.cs                  # Model kartu
│   ├── Decks.cs                 # Model deck kartu
│   ├── Dice.cs                  # Model dadu
│   ├── Money.cs                 # Model uang
│   ├── Player.cs                # Model pemain
│   └── Tile.cs                  # Model tile
├── 📁 Structs/
│   └── TilePos.cs               # Struct posisi tile (X, Y)
├── Program.cs                   # Entry point aplikasi
├── MonopolyVersion2.csproj      # File proyek
└── MonopolyVersion2.sln         # File solusi
```

---

## Komponen Utama

### 1. Program.cs (Entry Point)

Entry point aplikasi yang bertanggung jawab untuk:
- Menampilkan welcome screen
- Mengumpulkan informasi pemain
- Menginisialisasi komponen game
- Menjalankan game loop

```csharp
// Alur Program.cs:
1. Tampilkan welcome screen
2. Minta jumlah pemain (2-4)
3. Minta nama setiap pemain
4. Setup game components:
   - Board (papan permainan)
   - Dices (2 dadu 6 sisi)
   - Community Chest Deck
   - Chance Deck
5. Buat GameController
6. Mulai game loop
7. Tampilkan pemenang
```

### 2. GameController.cs (Controller Utama)

Controller pusat yang mengelola seluruh logika permainan.

#### Properties Utama:
| Property | Tipe | Deskripsi |
|----------|------|-----------|
| `Board` | `IBoard` | Papan permainan |
| `Players` | `List<IPlayer>` | Daftar pemain |
| `Dices` | `List<IDice>` | Daftar dadu |
| `PlayerAssets` | `Dictionary<IPlayer, List<IAsset>>` | Aset per pemain |
| `PlayerMoney` | `Dictionary<IPlayer, List<IMoney>>` | Uang per pemain |
| `TileAssets` | `Dictionary<ITile, IAsset?>` | Aset per tile |
| `CommunityChestDeck` | `IDecks` | Deck Dana Umum |
| `ChanceDeck` | `IDecks` | Deck Kesempatan |
| `CurrentTurn` | `int` | Giliran saat ini |
| `CurrentPlayer` | `IPlayer` | Pemain aktif saat ini |
| `IsGameOver` | `bool` | Status game selesai |
| `Winner` | `IPlayer?` | Pemenang permainan |

#### Events:
| Event | Signature | Deskripsi |
|-------|-----------|-----------|
| `OnMessage` | `Action<string>` | Pesan umum |
| `OnDiceRolled` | `Action<IPlayer, int, int>` | Dadu dilempar |
| `OnPlayerMoved` | `Action<IPlayer, ITile>` | Pemain bergerak |
| `OnPropertyBought` | `Action<IPlayer, IAsset>` | Properti dibeli |
| `OnRentPaid` | `Action<IPlayer, int>` | Sewa dibayar |
| `OnCardDrawn` | `Action<ICard>` | Kartu ditarik |
| `OnPlayerBankrupt` | `Action<IPlayer>` | Pemain bangkrut |
| `OnPlayerWins` | `Action<IPlayer>` | Pemain menang |

### 3. ConsoleView.cs (View Layer)

Menangani semua interaksi visual dengan pengguna melalui console.

#### Methods Utama:
| Method | Parameter | Return | Deskripsi |
|--------|-----------|--------|-----------|
| `DrawBoard` | `IBoard, List<IPlayer>` | `void` | Menggambar papan 11x11 |
| `ShowPlayerInfo` | `IPlayer, int` | `void` | Info pemain detail |
| `ShowAllPlayersInfo` | `List<IPlayer>, Dict<>` | `void` | Tabel semua pemain |
| `ShowMenu` | `string, List<string>` | `void` | Menampilkan menu opsi |
| `GetPlayerChoice` | `int` | `int` | Input pilihan pemain |
| `GetYesNo` | `string` | `bool` | Konfirmasi yes/no |
| `ShowDiceRoll` | `int, int` | `void` | Animasi hasil dadu |
| `ShowCard` | `ICard` | `void` | Tampilkan kartu |
| `ShowPropertyDetails` | `IAsset` | `void` | Detail properti |
| `ShowTradeOffer` | `...` | `void` | UI penawaran dagang |
| `ShowGameOver` | `IPlayer, int` | `void` | Layar akhir game |

---

## Game Flow (Alur Permainan)

### Diagram Alur Utama

```
┌──────────────────────────────────────────────────────────────────────┐
│                           MULAI GAME                                 │
└──────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
        ┌───────────────────────────────────────────────────────────┐
        │                    SETUP PEMAIN                           │
        │  • Input jumlah pemain (2-4)                              │
        │  • Input nama setiap pemain                               │
        │  • Setiap pemain mendapat $1500                           │
        └───────────────────────────────────────────────────────────┘
                                    │
                                    ▼
        ┌───────────────────────────────────────────────────────────┐
        │                  INISIALISASI GAME                        │
        │  • Setup Board (40 tiles)                                 │
        │  • Setup 2 dadu (6 sisi)                                  │
        │  • Setup deck Kesempatan (14 kartu)                       │
        │  • Setup deck Dana Umum (16 kartu)                        │
        └───────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌───────────────────────────────────────────────────────────────────────┐
│                         GAME LOOP                                     │
│  ┌────────────────────────────────────────────────────────────────┐  │
│  │                     PlayTurn()                                  │  │
│  │                                                                 │  │
│  │  ┌────────────────────────────────────────────────────────┐    │  │
│  │  │ 1. Cek apakah pemain bangkrut → Skip turn             │    │  │
│  │  └────────────────────────────────────────────────────────┘    │  │
│  │                          │                                      │  │
│  │                          ▼                                      │  │
│  │  ┌────────────────────────────────────────────────────────┐    │  │
│  │  │ 2. Tampilkan state game (board, info pemain)          │    │  │
│  │  └────────────────────────────────────────────────────────┘    │  │
│  │                          │                                      │  │
│  │                          ▼                                      │  │
│  │  ┌────────────────────────────────────────────────────────┐    │  │
│  │  │ 3. Cek apakah pemain di penjara                        │    │  │
│  │  │    → Ya: HandleJailOptions()                           │    │  │
│  │  │    → Tidak: Lanjut ke menu aksi                        │    │  │
│  │  └────────────────────────────────────────────────────────┘    │  │
│  │                          │                                      │  │
│  │                          ▼                                      │  │
│  │  ┌────────────────────────────────────────────────────────┐    │  │
│  │  │ 4. MENU AKSI:                                          │    │  │
│  │  │    [1] Lempar Dadu → RollDices() → MovePlayer()        │    │  │
│  │  │    [2] Lihat Properti → ShowPlayerProperties()         │    │  │
│  │  │    [3] Kelola Properti → ManagePlayerProperties()      │    │  │
│  │  │    [4] Berdagang → TradeFlow()                         │    │  │
│  │  │    [5] Akhiri Giliran                                   │    │  │
│  │  └────────────────────────────────────────────────────────┘    │  │
│  │                          │                                      │  │
│  │              [Jika Lempar Dadu Dipilih]                        │  │
│  │                          │                                      │  │
│  │                          ▼                                      │  │
│  │  ┌────────────────────────────────────────────────────────┐    │  │
│  │  │ 5. PROSES DADU:                                        │    │  │
│  │  │    • Lempar 2 dadu                                     │    │  │
│  │  │    • Jika dapat ganda (double):                        │    │  │
│  │  │      - 3x ganda berturut-turut → Masuk Penjara         │    │  │
│  │  │      - Selain itu → Boleh lempar lagi                  │    │  │
│  │  └────────────────────────────────────────────────────────┘    │  │
│  │                          │                                      │  │
│  │                          ▼                                      │  │
│  │  ┌────────────────────────────────────────────────────────┐    │  │
│  │  │ 6. PINDAH POSISI:                                      │    │  │
│  │  │    • MovePlayer(langkah)                               │    │  │
│  │  │    • Cek jika lewat MULAI → Terima $200                │    │  │
│  │  │    • Update posisi pemain                              │    │  │
│  │  └────────────────────────────────────────────────────────┘    │  │
│  │                          │                                      │  │
│  │                          ▼                                      │  │
│  │  ┌────────────────────────────────────────────────────────┐    │  │
│  │  │ 7. LANDING ACTION (OnLand):                            │    │  │
│  │  │    • Go → Berada di MULAI                              │    │  │
│  │  │    • Properti:                                         │    │  │
│  │  │      - Kosong → Tawaran beli                           │    │  │
│  │  │      - Dimiliki orang lain → Bayar sewa                │    │  │
│  │  │      - Milik sendiri → Tidak ada aksi                  │    │  │
│  │  │    • Dana Umum / Kesempatan → Ambil kartu              │    │  │
│  │  │    • Pajak → Bayar pajak ($200 atau $100)              │    │  │
│  │  │    • Ke Penjara → Masuk penjara                        │    │  │
│  │  │    • Parkir Gratis → Tidak ada aksi                    │    │  │
│  │  └────────────────────────────────────────────────────────┘    │  │
│  │                          │                                      │  │
│  │                          ▼                                      │  │
│  │  ┌────────────────────────────────────────────────────────┐    │  │
│  │  │ 8. CEK SALDO NEGATIF:                                  │    │  │
│  │  │    • Jika saldo < 0:                                   │    │  │
│  │  │      - Opsi: Jual rumah, Mortgage, atau Bangkrut       │    │  │
│  │  │    • Jika tidak bisa bayar → BANGKRUT                  │    │  │
│  │  └────────────────────────────────────────────────────────┘    │  │
│  │                          │                                      │  │
│  │                          ▼                                      │  │
│  │  ┌────────────────────────────────────────────────────────┐    │  │
│  │  │ 9. NEXT TURN:                                          │    │  │
│  │  │    • Pindah ke pemain berikutnya                       │    │  │
│  │  │    • Skip pemain yang bangkrut                         │    │  │
│  │  │    • Cek kondisi menang (1 pemain tersisa)             │    │  │
│  │  └────────────────────────────────────────────────────────┘    │  │
│  │                                                                 │  │
│  └─────────────────────────────────────────────────────────────────┘  │
│                                                                       │
│              [LOOP sampai IsGameOver = true]                          │
│                                                                       │
└───────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
        ┌───────────────────────────────────────────────────────────┐
        │                     GAME OVER                             │
        │  • Tampilkan pemenang                                     │
        │  • Tampilkan total uang & properti                        │
        │  • Ucapan terima kasih                                    │
        └───────────────────────────────────────────────────────────┘
```

---

## Dokumentasi Kode per File

### 📁 Enums/

#### `AssetCondition.cs`
```csharp
public enum AssetCondition
{
    Mortgage,   // Properti sedang digadaikan
    Normal      // Properti dalam kondisi normal
}
```
**Kegunaan:** Menentukan status gadai suatu properti. Properti yang di-mortgage tidak menghasilkan sewa.

---

#### `CardEffect.cs`
```csharp
public enum CardEffect
{
    ReceiveMoney,  // Menerima uang
    PayMoney,      // Membayar uang
    GoToJail,      // Langsung ke penjara
    GetOutJail,    // Kartu bebas penjara
    Move           // Pindah posisi
}
```
**Kegunaan:** Menentukan efek kartu Kesempatan dan Dana Umum.

---

#### `EffectType.cs`
```csharp
public enum EffectType
{
    Go,              // Tile MULAI (+$200)
    Tax,             // Tile Pajak
    GoToJail,        // Tile "Ke Penjara"
    Chance,          // Tile Kesempatan
    CommunityChest,  // Tile Dana Umum
    FreeParking,     // Tile Parkir Gratis
    Nothing          // Tidak ada efek khusus (properti)
}
```
**Kegunaan:** Menentukan efek yang terjadi saat pemain mendarat di tile tertentu.

---

#### `PlayerState.cs`
```csharp
public enum PlayerState
{
    Normal,    // Pemain aktif normal
    InJail,    // Pemain dalam penjara
    Bankrupt   // Pemain bangkrut
}
```
**Kegunaan:** Melacak status pemain dalam permainan.

---

#### `TilesType.cs`
```csharp
public enum TilesType
{
    Corner,    // Pojok papan (Go, Penjara, Parkir, Ke Penjara)
    Property,  // Properti yang bisa dibeli
    Railroad,  // Stasiun kereta api
    Utility,   // Perusahaan utilitas (Listrik, Air)
    Special    // Tile khusus (Kesempatan, Dana Umum, Pajak)
}
```
**Kegunaan:** Mengkategorikan jenis-jenis tile di papan.

---

#### `TypeAsset.cs`
```csharp
public enum TypeAsset
{
    RealEstate,     // Properti tanah/bangunan
    PublicService,  // Perusahaan utilitas
    Railroad        // Stasiun kereta api
}
```
**Kegunaan:** Mengkategorikan jenis aset yang bisa dimiliki pemain.

---

### 📁 Structs/

#### `TilePos.cs`
```csharp
public struct TilePos
{
    public int X;  // Koordinat horizontal
    public int Y;  // Koordinat vertikal

    public TilePos(int x, int y)
    {
        X = x;
        Y = y;
    }

    public override string ToString()
    {
        return $"({X}), {Y})";
    }
}
```
**Kegunaan:** Menyimpan koordinat posisi tile di grid papan 11x11.

---

### 📁 Interfaces/

#### `IPlayer.cs`
```csharp
public interface IPlayer
{
    string Name { get; set; }           // Nama pemain
    int PathIndex { get; set; }          // Posisi di jalur papan (0-39)
    IMoney Money { get; set; }           // Uang awal
    List<IAsset> Assets { get; set; }    // Daftar aset dimiliki
    ITile? CurrentTile { get; set; }     // Tile saat ini
    PlayerState PlayerState { get; set; } // Status pemain
}
```

---

#### `IAsset.cs`
```csharp
public interface IAsset
{
    string Name { get; set; }                // Nama properti
    TypeAsset TypeAsset { get; set; }        // Tipe aset
    AssetCondition AssetCondition { get; set; } // Kondisi (mortgage/normal)
    int Value { get; set; }                  // Harga properti
    IPlayer? Owner { get; set; }             // Pemilik
    int AmountHouse { get; set; }            // Jumlah rumah (0-5)
}
```

---

#### `IBoard.cs`
```csharp
public interface IBoard
{
    int Width { get; }           // Lebar papan
    int Height { get; }          // Tinggi papan
    ITile?[,] Grid { get; }      // Grid 2D tile
    List<ITile> Path { get; }    // Jalur tile berurutan
}
```

---

#### `ITile.cs`
```csharp
public interface ITile
{
    string Name { get; set; }        // Nama tile
    TilePos Pos { get; set; }        // Posisi di grid
    int? PathIndex { get; set; }     // Index di jalur (0-39)
    char Display { get; set; }       // Karakter display
    TilesType TilesType { get; set; } // Tipe tile
    EffectType EffectType { get; set; } // Efek tile
}
```

---

#### `ICard.cs`
```csharp
public interface ICard
{
    string Name { get; set; }         // Nama kartu
    string? Description { get; set; } // Deskripsi
    int Value { get; set; }           // Nilai (uang/posisi)
    CardEffect CardEffect { get; set; } // Efek kartu
}
```

---

#### `IDecks.cs`, `IDice.cs`, `IMoney.cs`
```csharp
// IDecks - Deck kartu
public interface IDecks { List<ICard> Cards { get; set; } }

// IDice - Dadu
public interface IDice { int Max { get; set; } }

// IMoney - Uang
public interface IMoney { int Balance { get; set; } }
```

---

#### `IView.cs`
```csharp
public interface IView
{
    // Display methods
    void ClearScreen();
    void DrawBoard(IBoard board, List<IPlayer> players);
    void ShowPlayerInfo(IPlayer player, int playerMoney);
    void ShowAllPlayersInfo(List<IPlayer> players, Dictionary<IPlayer, int> playerMoney);
    void ShowMessage(string message);
    void ShowError(string message);
    void ShowWarning(string message);
    void ShowSuccess(string message);
    void ShowDiceRoll(int dice1, int dice2);
    void ShowCard(ICard card);
    void ShowPropertyDetails(IAsset asset);
    void ShowTradeOffer(...);
    void ShowGameOver(IPlayer winner, int winnerMoney);
    void ShowWelcome();
    void ShowTurnHeader(string playerName);

    // Input methods
    void WaitForKeyPress();
    void ShowMenu(string title, List<string> options);
    int GetPlayerChoice(int maxOptions);
    string GetPlayerInput(string prompt);
    bool GetYesNo(string prompt);
    int GetPlayerCount(int min, int max);
    string GetPlayerName(int playerIndex);

    // Selection methods
    int? SelectFromPropertyList(List<IAsset> assets, string title, Func<IAsset, string> formatter);
    List<IAsset> SelectMultipleFromPropertyList(List<IAsset> assets, string prompt, Func<IAsset, string> formatter);
    IPlayer? SelectPlayer(List<IPlayer> players, string prompt, Func<IPlayer, string> formatter);
    int GetMoneyAmount(string prompt);
}
```

---

### 📁 Models/

#### `Player.cs`
```csharp
public class Player : IPlayer
{
    public string Name { get; set; }
    public int PathIndex { get; set; }           // Default: 0
    public PlayerState PlayerState { get; set; }  // Default: Normal
    public IMoney Money { get; set; }
    public List<IAsset> Assets { get; set; }     // Default: empty list
    public ITile? CurrentTile { get; set; }

    public Player(string name, IMoney money)
    {
        Name = name;
        PathIndex = 0;
        Money = money;
        Assets = new List<IAsset>();
        PlayerState = PlayerState.Normal;
        CurrentTile = null!;
    }
}
```

---

#### `Asset.cs`
```csharp
public class Asset : IAsset
{
    public string Name { get; set; }
    public TypeAsset TypeAsset { get; set; }
    public AssetCondition AssetCondition { get; set; }  // Default: Normal
    public int Value { get; set; }
    public IPlayer? Owner { get; set; }                  // Default: null
    public int AmountHouse { get; set; }                 // Default: 0

    public Asset(string name, TypeAsset typeAsset, int value)
    {
        Name = name;
        TypeAsset = typeAsset;
        Value = value;
        AssetCondition = AssetCondition.Normal;
        Owner = null;
        AmountHouse = 0;
    }
}
```

---

#### `Board.cs`
```csharp
public class Board : IBoard
{
    public int Width { get; set; }       // Default: 11
    public int Height { get; set; }      // Default: 11
    public ITile?[,] Grid { get; set; }  // 11x11 grid
    public List<ITile> Path { get; set; } // 40 tiles

    public Board(int width, int height)
    {
        Width = width;
        Height = height;
        Grid = new ITile[height, width];
        Path = new List<ITile>();
    }
}
```

---

#### `Tile.cs`
```csharp
public class Tile : ITile
{
    public string Name { get; set; }
    public TilePos Pos { get; set; }
    public int? PathIndex { get; set; }
    public char Display { get; set; }
    public EffectType EffectType { get; set; }
    public TilesType TilesType { get; set; }
    public TypeAsset TypeAsset { get; set; }        // Untuk tile properti
    public AssetCondition AssetCondition { get; set; }
    public int Value { get; set; }                   // Harga properti
    public IPlayer? Owner { get; set; }
    public int AmountHouse { get; set; }

    public Tile(TilePos pos, string name, char display = ' ', 
                TilesType type = TilesType.Special,
                EffectType effectType = EffectType.Nothing, 
                int? pathIndex = null)
    { ... }
}
```

---

#### `Card.cs`, `Decks.cs`, `Dice.cs`, `Money.cs`
```csharp
// Card
public class Card : ICard
{
    public string Name { get; set; }
    public CardEffect CardEffect { get; set; }
    public string? Description { get; set; }
    public int Value { get; set; }
}

// Decks
public class Decks : IDecks
{
    public List<ICard> Cards { get; set; }
}

// Dice
public class Dice : IDice
{
    public int Max { get; set; }  // Default: 6 (dadu 6 sisi)
}

// Money
public class Money : IMoney
{
    public int Balance { get; set; }  // Default: 1500
}
```

---

### 📁 Data/

#### `SetupBoard.cs`

File ini berisi konfigurasi lengkap papan permainan:

##### Fungsi Utama:

```csharp
// Membuat papan standar 40 tiles
public static Board CreateStandardBoard()

// Membuat deck Kesempatan (14 kartu)
public static Decks CreateChanceDeck()

// Membuat deck Dana Umum (16 kartu)
public static Decks CreateCommunityChestDeck()
```

##### Daftar Tile (40 total):

| Posisi | Nama | Tipe | Harga |
|--------|------|------|-------|
| 0 | MULAI (GO) | Corner | - |
| 1 | Aceh | Property | $60 |
| 2 | Dana Umum | Special | - |
| 3 | Medan | Property | $60 |
| 4 | Pajak Penghasilan | Tax | -$200 |
| 5 | Stasiun Gambir | Railroad | $200 |
| 6 | Palembang | Property | $100 |
| 7 | Kesempatan | Special | - |
| 8 | Padang | Property | $100 |
| 9 | Pekanbaru | Property | $120 |
| 10 | Penjara | Corner | - |
| 11 | Bandung | Property | $140 |
| 12 | Perusahaan Listrik | Utility | $150 |
| 13 | Bogor | Property | $140 |
| 14 | Tangerang | Property | $160 |
| 15 | Stasiun Pasar Senen | Railroad | $200 |
| 16 | Bekasi | Property | $180 |
| 17 | Dana Umum | Special | - |
| 18 | Depok | Property | $180 |
| 19 | Jakarta | Property | $200 |
| 20 | Parkir Gratis | Corner | - |
| 21 | Semarang | Property | $220 |
| 22 | Kesempatan | Special | - |
| 23 | Yogyakarta | Property | $220 |
| 24 | Solo | Property | $240 |
| 25 | Stasiun Jatinegara | Railroad | $200 |
| 26 | Malang | Property | $260 |
| 27 | Kediri | Property | $260 |
| 28 | Perusahaan Air | Utility | $150 |
| 29 | Surabaya | Property | $280 |
| 30 | Ke Penjara | Corner | - |
| 31 | Denpasar | Property | $300 |
| 32 | Mataram | Property | $300 |
| 33 | Dana Umum | Special | - |
| 34 | Makassar | Property | $320 |
| 35 | Stasiun Manggarai | Railroad | $200 |
| 36 | Kesempatan | Special | - |
| 37 | Manado | Property | $350 |
| 38 | Pajak Mewah | Tax | -$100 |
| 39 | Jayapura | Property | $400 |

---

### 📁 Controllers/

#### `GameController.cs` - Dokumentasi Lengkap Methods

##### Konstanta:
```csharp
private const int GO_SALARY = 200;       // Uang saat lewat MULAI
private const int JAIL_POSITION = 10;    // Posisi penjara
private const int JAIL_FEE = 50;         // Biaya keluar penjara
private const int TAX_AMOUNT = 200;      // Pajak penghasilan
private const int LUXURY_TAX = 100;      // Pajak mewah
```

##### Methods Utama:

| Method | Deskripsi |
|--------|-----------|
| `StartGame()` | Memulai permainan, menampilkan pesan awal |
| `PlayTurn()` | Menjalankan satu giliran pemain |
| `NextTurn()` | Pindah ke giliran pemain berikutnya |
| `RollDices()` | Melempar 2 dadu dan mengembalikan hasilnya |
| `MovePlayer(int steps)` | Menggerakkan pemain sebanyak langkah |
| `MovePlayerToPosition(int pos)` | Memindahkan pemain ke posisi tertentu |
| `OnLand()` | Mengeksekusi efek saat mendarat di tile |
| `SendToJail()` | Mengirim pemain ke penjara |

##### Methods Manajemen Uang:

| Method | Parameter | Return | Deskripsi |
|--------|-----------|--------|-----------|
| `AddMoney` | `IPlayer, int` | `bool` | Menambah uang pemain |
| `SubtractMoney` | `IPlayer, int` | `bool` | Mengurangi uang pemain |
| `GetPlayerMoney` | `IPlayer` | `int` | Mendapatkan total uang |

##### Methods Penjara:

| Method | Deskripsi |
|--------|-----------|
| `HandleJailTurn()` | Menangani giliran di penjara |
| `HandleJailOptions()` | Menu opsi untuk keluar penjara |
| `PayJailFee()` | Bayar $50 untuk keluar |
| `UseGetOutOfJailCard()` | Gunakan kartu bebas penjara |
| `TryRollDoublesInJail()` | Coba lempar ganda untuk bebas |
| `GetJailTurns(IPlayer)` | Mendapatkan jumlah giliran di penjara |
| `HasGetOutOfJailCard(IPlayer)` | Cek apakah punya kartu bebas |

##### Methods Properti:

| Method | Deskripsi |
|--------|-----------|
| `PlayerBuyAsset(IAsset)` | Pemain membeli properti |
| `PlayerMortgageAsset(IPlayer, IAsset)` | Mortgage properti |
| `PlayerUnmortgageAsset(IPlayer, IAsset)` | Unmortgage properti |
| `PlayerAddHouse(IAsset)` | Bangun rumah |
| `PlayerSellHouse(IAsset)` | Jual rumah |
| `CalculateRent(IAsset)` | Hitung sewa properti |
| `HandlePropertyTile(ITile)` | Handle mendarat di properti |

##### Methods Trading:

| Method | Deskripsi |
|--------|-----------|
| `TradeFlow()` | Alur lengkap perdagangan |
| `PlayerProposeTrade(...)` | Eksekusi perdagangan |

##### Methods Kartu:

| Method | Deskripsi |
|--------|-----------|
| `DrawCardFromDeck(IDecks)` | Ambil kartu dari deck |
| `ApplyCardEffect(ICard)` | Terapkan efek kartu |
| `GetAndApplyDeck(IDecks)` | Ambil dan terapkan kartu |

##### Methods Kebangkrutan:

| Method | Deskripsi |
|--------|-----------|
| `CheckIsBankrupt(IPlayer)` | Cek apakah pemain bangkrut |
| `HandleNegativeBalance()` | Handle saldo negatif |
| `CalculatePlayerTotalAssetsValue(IPlayer)` | Hitung nilai total aset |
| `GetMortgageValue(IAsset)` | Nilai mortgage (50% harga) |
| `GetUnmortgageCost(IAsset)` | Biaya unmortgage (110% mortgage) |

---

### 📁 ConsoleView/

#### `View.cs` - Dokumentasi Lengkap

Konstanta tampilan:
```csharp
private const int TILE_WIDTH = 14;   // Lebar tile
private const int TILE_HEIGHT = 4;   // Tinggi tile
```

##### Visualisasi Papan:
- Papan digambar dalam grid 11x11
- Setiap tile berukuran 14x4 karakter
- Pemain ditandai dengan `[X]` (huruf pertama nama)
- Properti menampilkan nama singkat dan informasi

##### Contoh Tampilan:
```
+------------+------------+------------+...
|MULAI    [A]|Aceh        |Dana Umum   |
|+$200       |            |            |
+------------+------------+------------+...
```

---

## Mekanik Permainan

### 1. Sistem Sewa Properti

#### RealEstate (Properti Tanah):
```
Base Rent = Harga Properti / 10

Rumah 1: Base Rent × 5
Rumah 2: Base Rent × 15  
Rumah 3: Base Rent × 45
Rumah 4: Base Rent × 80
Hotel (5): Base Rent × 100
```

**Contoh:** Jakarta ($200)
- Base rent: $20
- 1 rumah: $100
- 2 rumah: $300
- 3 rumah: $900
- 4 rumah: $1600
- Hotel: $2000

#### Railroad (Stasiun):
```
1 stasiun: $25
2 stasiun: $50
3 stasiun: $100
4 stasiun: $200
```

#### Utility (Perusahaan):
```
1 utility: $25
2 utility: $50
```

---

### 2. Sistem Penjara

#### Cara Masuk Penjara:
1. Mendarat di tile "Ke Penjara" (posisi 30)
2. Mengambil kartu "Masuk Penjara"
3. Lempar 3x ganda berturut-turut

#### Cara Keluar Penjara:
1. **Bayar $50** - Langsung bayar dan bebas
2. **Lempar Dadu Ganda** - Jika dapat ganda, bebas dan bergerak
3. **Gunakan Kartu Bebas Penjara** - Jika punya kartu
4. **Otomatis Setelah 3 Giliran** - Harus bayar $50

---

### 3. Sistem Mortgage

#### Mortgage Properti:
- Nilai mortgage = 50% harga properti
- Properti tidak menghasilkan sewa saat di-mortgage
- Harus jual semua rumah sebelum mortgage

#### Unmortgage Properti:
- Biaya = 110% nilai mortgage
- **Contoh:** Properti $200
  - Mortgage: $100
  - Unmortgage: $110

---

### 4. Sistem Bangunan

#### Bangun Rumah:
- Biaya = 50% harga properti
- Maksimum 5 (rumah ke-5 = Hotel)
- Hanya untuk properti RealEstate

#### Jual Rumah:
- Harga jual = 50% harga bangunan
- = 25% harga properti

---

### 5. Sistem Kebangkrutan

Pemain dinyatakan bangkrut jika:
1. Saldo negatif
2. Tidak bisa membayar meski sudah mortgage semua properti

Saat bangkrut:
- Semua properti dikembalikan ke bank
- Pemain tidak ikut bermain lagi
- Jika tersisa 1 pemain → MENANG

---

### 6. Sistem Perdagangan

Pemain bisa bertukar:
- **Properti** (yang tidak di-mortgage dan tidak ada rumah)
- **Uang**

Proses:
1. Pilih pemain target
2. Pilih properti untuk ditawarkan
3. Masukkan jumlah uang yang ditawarkan
4. Pilih properti yang diminta
5. Masukkan jumlah uang yang diminta
6. Konfirmasi dari kedua pihak

---

## Skenario Penggunaan

### Skenario 1: Memulai Permainan Baru

```
1. Jalankan aplikasi
2. Layar welcome muncul, tekan tombol apa saja
3. Masukkan jumlah pemain (2-4): 3
4. Masukkan nama Pemain 1: Andi
5. Masukkan nama Pemain 2: Budi  
6. Masukkan nama Pemain 3: Citra
7. Game dimulai, giliran pertama: Andi
```

---

### Skenario 2: Membeli Properti

```
Situasi: Andi mendarat di Bandung (belum ada pemilik)

1. Andi memilih [1] Lempar Dadu
2. Dadu menunjukkan 4 dan 3 (total 7)
3. Andi bergerak ke Bandung
4. Muncul detail properti:
   - Harga: $140
   - Tipe: RealEstate
   - Sewa dasar: $14
5. Prompt: "Beli Bandung seharga $140? (y/t)"
6. Andi memilih "y"
7. Bandung sekarang milik Andi
8. Uang Andi: $1500 → $1360
```

---

### Skenario 3: Membayar Sewa

```
Situasi: Budi mendarat di Bandung milik Andi

1. Budi melempar dadu dan mendarat di Bandung
2. Sistem menghitung sewa:
   - Base rent: $14 (tidak ada rumah)
3. Budi membayar $14 ke Andi
4. Uang Budi: $1360 → $1346
5. Uang Andi: $1360 → $1374
```

---

### Skenario 4: Membangun Rumah

```
Situasi: Andi ingin membangun rumah di Bandung

1. Pada giliran Andi, pilih [3] Kelola Properti
2. Muncul menu:
   [1] Bangun Rumah
   [2] Jual Rumah
   [3] Mortgage Properti
   [4] Unmortgage Properti
   [5] Kembali
3. Pilih [1] Bangun Rumah
4. Muncul list properti:
   [1] Bandung - Biaya rumah: $70 - Saat ini: 0 rumah
5. Pilih [1]
6. Rumah dibangun!
7. Uang Andi: $1374 → $1304
8. Bandung sekarang: 1 rumah, sewa $70
```

---

### Skenario 5: Masuk Penjara

```
Situasi: Citra mendarat di "Ke Penjara"

1. Citra melempar dadu dan berjalan
2. Mendarat di posisi 30 "Ke Penjara"
3. Sistem mengirim Citra ke penjara (posisi 10)
4. Status Citra: InJail
5. Giliran selanjutnya Citra akan dapat opsi:
   [1] Coba lempar ganda
   [2] Bayar $50 untuk keluar
   [3] Gunakan kartu Bebas Penjara (jika ada)
```

---

### Skenario 6: Keluar Penjara dengan Ganda

```
Situasi: Citra di penjara giliran ke-1

1. Muncul: "Citra di Penjara! (Giliran 1/3)"
2. Citra pilih [1] Coba lempar ganda
3. Dadu: 4 dan 4 → GANDA!
4. Citra keluar dari penjara
5. Citra bergerak 8 langkah dari penjara
6. Lanjut normal
```

---

### Skenario 7: Mortgage Properti

```
Situasi: Budi butuh uang mendesak

1. Budi pilih [3] Kelola Properti
2. Pilih [3] Mortgage Properti
3. Muncul list properti (tanpa rumah):
   [1] Jakarta - Nilai mortgage: $100
4. Pilih [1]
5. Jakarta di-mortgage
6. Uang Budi: $500 → $600
7. Jakarta tidak menghasilkan sewa sampai di-unmortgage
```

---

### Skenario 8: Perdagangan Antar Pemain

```
Situasi: Andi ingin properti Budi

1. Andi pilih [4] Berdagang
2. Muncul list pemain:
   [1] Budi - $600 - 2 properti
   [2] Citra - $1200 - 1 properti
3. Pilih [1] Budi

4. "Pilih properti Anda untuk ditawarkan:"
   [1] Bandung
   → Pilih 1

5. "Masukkan jumlah uang untuk ditawarkan: $" 
   → Masukkan 100

6. "Pilih properti Budi yang Anda inginkan:"
   [1] Surabaya
   [2] Semarang
   → Pilih 1,2

7. "Masukkan jumlah uang yang diminta: $"
   → Masukkan 0

8. Tampilkan ringkasan penawaran
9. "Apakah Budi menerima perdagangan ini? (y/t)"
   → y

10. Perdagangan selesai!
    - Andi mendapat Surabaya dan Semarang
    - Budi mendapat Bandung dan $100
```

---

### Skenario 9: Bangkrut

```
Situasi: Andi tidak bisa bayar sewa $500

1. Andi mendarat di properti dengan sewa $500
2. Uang Andi: $200
3. Muncul: "Andi memiliki saldo negatif!"
4. Menu:
   [1] Jual Rumah
   [2] Mortgage Properti
   [3] Nyatakan Bangkrut

5. Andi mortgage semua properti, masih kurang
6. Andi pilih [3] Nyatakan Bangkrut

7. "Andi BANGKRUT!"
8. Semua properti Andi kembali ke bank
9. Andi tidak ikut bermain lagi

10. Jika tersisa 1 pemain → GAME OVER, pemain tersisa MENANG
```

---

### Skenario 10: Memenangkan Permainan

```
Situasi: Tersisa 1 pemain aktif

1. Budi dan Citra sudah bangkrut
2. Hanya Andi tersisa

3. Layar Game Over:
   ================================
   |        GAME SELESAI!         |
   ================================
   | PEMENANG: Andi               |
   | Total Uang: $3,500           |
   | Properti: 8                  |
   ================================

4. "Terima kasih sudah bermain Monopoly!"
5. "Tekan tombol apa saja untuk keluar..."
```

---

## Cara Menjalankan

### Prasyarat:
- .NET SDK 6.0 atau lebih baru

### Langkah-langkah:

```bash
# 1. Clone repository atau navigasi ke folder proyek
cd MonopolyVersion2

# 2. Build proyek
dotnet build

# 3. Jalankan aplikasi
dotnet run
```

### Kontrol Permainan:

| Input | Fungsi |
|-------|--------|
| `1-9` | Memilih opsi menu |
| `y` / `t` | Konfirmasi ya/tidak |
| `Enter` | Lanjutkan |
| `0` | Batal/Kembali (pada beberapa menu) |

---

## Catatan Pengembang

### Prinsip SOLID yang Diterapkan:

1. **Single Responsibility** - Setiap class memiliki satu tanggung jawab
2. **Open/Closed** - Interface memungkinkan ekstensi tanpa modifikasi
3. **Liskov Substitution** - Model dapat diganti dengan implementasi lain
4. **Interface Segregation** - Interface terpisah untuk setiap kebutuhan
5. **Dependency Inversion** - Controller bergantung pada abstraksi (interface)

### Extensibility:

- Mudah menambah jenis tile baru melalui `TilesType` dan `EffectType`
- View dapat diganti (misalnya dari console ke GUI) dengan mengimplementasi `IView`
- Kartu bisa ditambah di `SetupBoard.cs`

---

## Lisensi

Proyek ini dibuat untuk keperluan edukasi Bootcamp Formulatrix.

---

*Dokumentasi ini dibuat pada 6 Februari 2026*
