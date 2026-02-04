# Console Monopoly App - Dokumentasi Lengkap

## Daftar Isi

1. [Gambaran Umum Aplikasi](#1-gambaran-umum-aplikasi)
2. [GameController.cs](#2-gamecontrollercs)
3. [Program.cs](#3-programcs)
4. [ConsoleView.cs](#4-consoleviewcs)
5. [Models](#5-models)
6. [Flow Permainan](#6-flow-permainan)
7. [Cara Menjalankan Aplikasi](#7-cara-menjalankan-aplikasi)

---

# 1. Gambaran Umum Aplikasi

## 1.1 Deskripsi

Console Monopoly App adalah implementasi permainan Monopoly klasik dalam bentuk aplikasi console menggunakan bahasa pemrograman C#. Aplikasi ini mensimulasikan pengalaman bermain Monopoly dengan fitur-fitur lengkap termasuk:

- **Papan permainan standard 40 tile** dengan properti, railroad, utility, dan tile spesial
- **Sistem jail** dengan opsi pembayaran, kartu, atau roll doubles
- **Pembelian dan penjualan properti**
- **Pembangunan rumah dan hotel**
- **Sistem mortgage**
- **Trading antar pemain**
- **Kartu Community Chest dan Chance**
- **Deteksi bankruptcy dan game over**

## 1.2 Arsitektur MVC

Aplikasi ini menggunakan pola arsitektur **Model-View-Controller (MVC)**:

```
┌─────────────────────────────────────────────────────────────┐
│                        CONTROLLER                            │
│                    GameController.cs                         │
│  - Mengelola logika permainan                               │
│  - Mengontrol alur giliran pemain                           │
│  - Memproses aksi pemain                                    │
│  - Mengirim notifikasi melalui events                       │
└─────────────────────────────────────────────────────────────┘
                              │
           ┌──────────────────┴──────────────────┐
           ▼                                      ▼
┌─────────────────────────┐        ┌─────────────────────────┐
│         MODEL           │        │          VIEW           │
│  - Player.cs            │        │    ConsoleView.cs       │
│  - Asset.cs             │        │  - Rendering papan      │
│  - Board.cs             │        │  - Menampilkan info     │
│  - Tile.cs              │        │  - Input dari user      │
│  - Card.cs              │        │  - Menu dan dialog      │
│  - Dice.cs              │        │                         │
│  - Money.cs             │        │                         │
│  - Decks.cs             │        │                         │
└─────────────────────────┘        └─────────────────────────┘
```

## 1.3 Struktur Folder

```
ConsoleMonopolyApp/
├── Controllers/
│   └── GameController.cs      # Logic permainan utama
├── Data/
│   └── BoardPreset.cs         # Konfigurasi papan standard
├── Enums/
│   ├── AssetsCondition.cs     # NORMAL, MORTGAGED
│   ├── CardEffect.cs          # RECEIVE_MONEY, PAY_MONEY, dll
│   ├── EffectType.cs          # GO, TAX, CHANCE, dll
│   ├── PlayerState.cs         # Normal, InJail, Bankrupt
│   ├── TilesType.cs           # PROPERTY, CORNER, SPECIAL
│   └── TypeAsset.cs           # REAL_ESTATE, RAILROAD, PUBLIC_SERVICE
├── Interfaces/
│   ├── IAsset.cs
│   ├── IBoard.cs
│   ├── ICard.cs
│   ├── IDecks.cs
│   ├── IDice.cs
│   ├── IMoney.cs
│   ├── IPlayer.cs
│   └── ITile.cs
├── Models/
│   ├── Asset.cs               # Properti, railroad, utility
│   ├── Board.cs               # Papan permainan
│   ├── Card.cs                # Kartu Community Chest/Chance
│   ├── Decks.cs               # Kumpulan kartu
│   ├── Dice.cs                # Dadu
│   ├── Money.cs               # Uang pemain
│   ├── Player.cs              # Data pemain
│   └── Tile.cs                # Tile di papan
├── Structs/
│   └── TilePos.cs             # Posisi tile (x, y)
├── Views/
│   └── ConsoleView.cs         # Tampilan console
├── Program.cs                 # Entry point & UI flow
└── ConsoleMonopolyApp.csproj  # Project file
```

---

# 2. GameController.cs

**Lokasi:** `Controllers/GameController.cs`  
**Total Baris:** 680

## 2.1 Deskripsi & Tanggung Jawab

`GameController` adalah inti dari aplikasi yang mengelola seluruh logika permainan Monopoly. Class ini bertanggung jawab untuk:

- Menginisialisasi dan memulai permainan
- Mengatur giliran pemain
- Memproses pergerakan pemain di papan
- Menangani pembelian dan penjualan properti
- Mengelola sistem jail
- Memproses kartu Community Chest dan Chance
- Mendeteksi kondisi bankruptcy dan game over
- Mengirim notifikasi ke View melalui events

## 2.2 Properties (11 properties)

```csharp
public IBoard Board { get; }                                    // Papan permainan
public List<IPlayer> Players { get; }                          // Daftar pemain
public List<IDice> Dices { get; }                              // Daftar dadu
public Dictionary<IPlayer, List<IAsset>> PlayerAssets { get; } // Mapping pemain-aset
public Dictionary<ITile, IAsset?> TileAssets { get; }          // Mapping tile-aset
public IDecks CommunityChestDeck { get; }                      // Deck Community Chest
public IDecks ChanceDeck { get; }                              // Deck Chance
public int CurrentTurn { get; private set; }                   // Nomor giliran saat ini
public IPlayer CurrentPlayer => Players[CurrentTurn % Players.Count]; // Pemain aktif
public bool IsGameOver { get; private set; }                   // Status game over
public IPlayer? Winner { get; private set; }                   // Pemenang (jika ada)
public int LastDiceRoll { get; private set; }                  // Hasil roll terakhir
```

## 2.3 Konstanta (5 konstanta)

```csharp
private const int GO_SALARY = 200;      // Gaji saat melewati GO
private const int JAIL_POSITION = 10;   // Posisi jail di papan
private const int JAIL_FEE = 50;        // Biaya keluar dari jail
private const int TAX_AMOUNT = 200;     // Income Tax
private const int LUXURY_TAX = 100;     // Luxury Tax
```

## 2.4 Events (8 events)

```csharp
public event Action<string>? OnMessage;              // Pesan umum
public event Action<IPlayer, int, int>? OnDiceRolled; // Hasil roll dadu
public event Action<IPlayer, ITile>? OnPlayerMoved;   // Pemain berpindah
public event Action<IPlayer, IAsset>? OnPropertyBought; // Properti dibeli
public event Action<IPlayer, int>? OnRentPaid;        // Rent dibayar
public event Action<ICard>? OnCardDrawn;              // Kartu diambil
public event Action<IPlayer>? OnPlayerBankrupt;       // Pemain bangkrut
public event Action<IPlayer>? OnPlayerWins;           // Pemain menang
```

## 2.5 Constructor

**Lokasi:** Baris 37-67

```csharp
public GameController(IBoard board, List<IPlayer> players, List<IDice> dices, 
                      IDecks communityChestDeck, IDecks chanceDeck)
```

**Penjelasan Logic:**
1. Validasi jumlah pemain (harus 2-4 pemain)
2. Inisialisasi semua properties
3. Set `CurrentTurn = 0`, `IsGameOver = false`
4. Buat dictionary untuk PlayerAssets dan TileAssets
5. Set posisi awal semua pemain di tile GO (RouteIndex = 0)
6. Mapping setiap tile ke asset-nya

---

## 2.6 Method - Game Flow

### 2.6.1 StartGame()

**Lokasi:** Baris 69-74

```csharp
public void StartGame()
{
    OnMessage?.Invoke("Game Started! Welcome to Monopoly!");
    OnMessage?.Invoke($"Players: {string.Join(", ", Players.Select(p => p.Name))}");
    OnMessage?.Invoke($"{CurrentPlayer.Name}'s turn!");
}
```

**Deskripsi:** Mengirim notifikasi awal permainan ke View.

### 2.6.2 NextTurn()

**Lokasi:** Baris 76-94

```csharp
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
```

**Penjelasan Logic:**
1. Increment `CurrentTurn` dan skip pemain yang bankrupt
2. Cek apakah hanya tersisa 1 pemain aktif (game over)
3. Jika game over, set Winner dan invoke event
4. Jika belum, announce giliran pemain selanjutnya

### 2.6.3 RollDice()

**Lokasi:** Baris 96-108

```csharp
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
```

**Deskripsi:** Roll 2 dadu dan return tuple hasil. Juga menyimpan total di `LastDiceRoll`.

---

## 2.7 Method - Jail System

### 2.7.1 HandleJailTurn()

**Lokasi:** Baris 121-130

```csharp
public bool HandleJailTurn()
{
    if (CurrentPlayer.State != PlayerState.InJail)
        return false;

    CurrentPlayer.JailTurns++;
    return true;
}
```

### 2.7.2 PayJailFee()

**Lokasi:** Baris 132-147

```csharp
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
```

### 2.7.3 UseGetOutOfJailCard()

**Lokasi:** Baris 149-159

```csharp
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
```

### 2.7.4 TryRollDoublesForJail()

**Lokasi:** Baris 161-193

```csharp
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
```

---

## 2.8 Method - Movement

### 2.8.1 MovePlayer(int steps)

**Lokasi:** Baris 195-214

```csharp
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
```

### 2.8.2 MovePlayerToPosition(int position)

**Lokasi:** Baris 216-230

```csharp
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
```

### 2.8.3 SendToJail()

**Lokasi:** Baris 232-239

```csharp
public void SendToJail()
{
    CurrentPlayer.RouteIndex = JAIL_POSITION;
    CurrentPlayer.CurrentTile = Board.Route[JAIL_POSITION];
    CurrentPlayer.State = PlayerState.InJail;
    CurrentPlayer.JailTurns = 0;
    OnMessage?.Invoke($"{CurrentPlayer.Name} was sent to Jail!");
}
```

---

## 2.9 Method - Tile Landing

### 2.9.1 OnLand()

**Lokasi:** Baris 241-290

```csharp
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
            OnMessage?.Invoke($"{CurrentPlayer.Name} is just visiting Jail.");
            break;

        case EffectType.FREE_PARKING:
            OnMessage?.Invoke($"{CurrentPlayer.Name} is relaxing at Free Parking.");
            break;

        case EffectType.NOTHING:
            if (tile.Asset != null)
            {
                HandlePropertyTile(tile);
            }
            break;
    }
}
```

### 2.9.2 HandlePropertyTile(ITile tile)

**Lokasi:** Baris 292-330

Method private yang menangani landing di tile properti:
- Jika tidak ada owner: tampilkan info untuk pembelian
- Jika ada owner (bukan pemain aktif): bayar rent
- Jika milik pemain aktif: tampilkan pesan kepemilikan

---

## 2.10 Method - Asset Management

### 2.10.1 PlayerBuyAsset(IAsset asset)

**Lokasi:** Baris 350-369

```csharp
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
```

### 2.10.2 PlayerMortgageAsset(IPlayer player, IAsset asset)

**Lokasi:** Baris 371-396

Validasi: owner, belum di-mortgage, tidak ada rumah. Lalu set condition ke MORTGAGED dan tambahkan uang.

### 2.10.3 PlayerUnmortgageAsset(IPlayer player, IAsset asset)

**Lokasi:** Baris 398-422

Validasi: owner, sudah di-mortgage, cukup uang. Bayar 110% dari mortgage value.

### 2.10.4 PlayerAddHouse(IAsset asset)

**Lokasi:** Baris 424-461

```csharp
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
```

### 2.10.5 PlayerSellHouse(IAsset asset)

**Lokasi:** Baris 463-482

Jual rumah dengan harga 50% dari house cost.

### 2.10.6 OwnsFullColorGroup(IPlayer player, int colorGroup)

**Lokasi:** Baris 484-489

```csharp
private bool OwnsFullColorGroup(IPlayer player, int colorGroup)
{
    int required = (colorGroup == 1 || colorGroup == 8) ? 2 : 3;
    int count = player.Assets.Count(a => a.ColorGroup == colorGroup);
    return count >= required;
}
```

---

## 2.11 Method - Trading

### 2.11.1 PlayerProposeTrade()

**Lokasi:** Baris 491-558

```csharp
public bool PlayerProposeTrade(IPlayer player1, IPlayer player2, 
                                List<IAsset> offer1, int money1,
                                List<IAsset> offer2, int money2)
```

**Penjelasan Logic:**
1. Validasi kepemilikan semua aset yang ditawarkan
2. Validasi ketersediaan uang
3. Transfer aset dari player1 ke player2 dan sebaliknya
4. Transfer uang sesuai penawaran
5. Update dictionary PlayerAssets

---

## 2.12 Method - Card/Deck

### 2.12.1 GetAndApplyDeck(IDecks deck)

**Lokasi:** Baris 560-607

```csharp
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
                MovePlayer(card.Value);
            }
            else
            {
                MovePlayerToPosition(card.Value);
            }
            OnLand();
            break;
    }
}
```

---

## 2.13 Method - Utility

### 2.13.1 CheckIsBankrupt(IPlayer player)

**Lokasi:** Baris 609-642

Cek apakah pemain bangkrut. Jika ya:
- Set state ke Bankrupt
- Return semua aset ke bank
- Cek apakah game over

### 2.13.2 CalculatePlayerTotalAssetsValue(IPlayer player)

**Lokasi:** Baris 644-660

Hitung total nilai aset pemain termasuk nilai mortgage dan rumah.

### 2.13.3 GetActivePlayers()

**Lokasi:** Baris 662-665

```csharp
public List<IPlayer> GetActivePlayers()
{
    return Players.Where(p => p.State != PlayerState.Bankrupt).ToList();
}
```

### 2.13.4 CanBuyCurrentProperty()

**Lokasi:** Baris 667-673

Cek apakah pemain bisa membeli properti di tile saat ini.

### 2.13.5 GetCurrentTileAsset()

**Lokasi:** Baris 675-678

Return asset di tile saat ini pemain berada.

---

# 3. Program.cs

**Lokasi:** `Program.cs`  
**Total Baris:** 582

## 3.1 Deskripsi & Tanggung Jawab

`Program.cs` adalah entry point aplikasi yang menghubungkan Controller dengan View. File ini bertanggung jawab untuk:

- Menampilkan welcome screen
- Setup pemain
- Inisialisasi komponen game
- Menjalankan game loop utama
- Menangani input user
- Mengkoordinasikan interaksi UI

## 3.2 Fields

```csharp
private static GameController? _game;   // Instance GameController
private static ConsoleView? _view;       // Instance ConsoleView
```

## 3.3 Main() - Entry Point

**Lokasi:** Baris 15-49

```csharp
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
```

## 3.4 SetupPlayers()

**Lokasi:** Baris 51-77

```csharp
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
```

## 3.5 RunGameLoop()

**Lokasi:** Baris 79-205

Main game loop yang:
1. Cek apakah game over
2. Skip pemain bankrupt  
3. Tampilkan state game
4. Handle jail turn jika pemain di jail
5. Tampilkan menu aksi (Roll, View Properties, Manage, Trade, End Turn)
6. Proses doubles (roll lagi jika dapat doubles, jail jika 3x doubles)
7. Handle pembelian properti setelah landing
8. Post-turn actions
9. Lanjut ke giliran berikutnya

## 3.6 HandleJailTurn()

**Lokasi:** Baris 207-239

```csharp
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
```

## 3.7 HandlePropertyPurchase()

**Lokasi:** Baris 241-263

Menampilkan detail properti dan tanya user apakah ingin membeli.

## 3.8 ViewProperties()

**Lokasi:** Baris 265-291

Menampilkan daftar properti yang dimiliki pemain saat ini.

## 3.9 ManageProperties()

**Lokasi:** Baris 293-332

Menu untuk mengelola properti:
1. Build House
2. Sell House
3. Mortgage Property
4. Unmortgage Property
5. Go Back

## 3.10 BuildHouse()

**Lokasi:** Baris 334-363

Memilih properti untuk dibangun rumah.

## 3.11 SellHouse()

**Lokasi:** Baris 365-392

Memilih properti untuk menjual rumah.

## 3.12 MortgageProperty()

**Lokasi:** Baris 394-421

Memilih properti untuk di-mortgage.

## 3.13 UnmortgageProperty()

**Lokasi:** Baris 423-450

Memilih properti untuk di-unmortgage.

## 3.14 HandleTrade()

**Lokasi:** Baris 452-507

```csharp
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

    // Select player to trade with
    // Select properties to offer
    // Enter money to offer
    // Select properties to request
    // Enter money to request
    // Show trade summary
    // Ask for confirmation
    // Execute trade if accepted
}
```

## 3.15 SelectPropertiesFromPlayer()

**Lokasi:** Baris 509-540

Helper untuk memilih properti dari pemain (digunakan dalam trading).

## 3.16 PostTurnActions()

**Lokasi:** Baris 542-580

Menangani situasi ketika pemain memiliki saldo negatif:
1. Tampilkan warning
2. Loop sampai saldo positif atau bankrupt
3. Opsi: Sell House, Mortgage Property, Declare Bankruptcy

---

# 4. ConsoleView.cs

**Lokasi:** `Views/ConsoleView.cs`  
**Total Baris:** 506

## 4.1 Deskripsi & Tanggung Jawab

`ConsoleView` menangani semua tampilan dan input console. Bertanggung jawab untuk:

- Rendering papan permainan dengan warna
- Menampilkan informasi pemain
- Menampilkan menu dan dialog
- Menerima input dari user
- Menampilkan kartu, dadu, dan animasi ASCII

## 4.2 Konstanta & Konfigurasi

```csharp
private const int TILE_WIDTH = 12;   // Lebar tile dalam karakter
private const int TILE_HEIGHT = 3;    // Tinggi tile dalam baris

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
```

## 4.3 ClearScreen()

**Lokasi:** Baris 25-28

```csharp
public void ClearScreen()
{
    Console.Clear();
}
```

## 4.4 DrawBoard()

**Lokasi:** Baris 30-64

Menggambar papan 11x11 dengan loop nested:
- Outer loop: iterasi baris (y)
- Middle loop: iterasi 3 baris per tile (TILE_HEIGHT)
- Inner loop: iterasi kolom (x)

## 4.5 DrawTileLine()

**Lokasi:** Baris 66-139

Menggambar satu baris dari tile:
- Line 0: Border atas (┌──────────┐)
- Line 1: Nama tile + marker pemain
- Line 2: Harga/info + owner symbol

## 4.6 DrawCenterArea()

**Lokasi:** Baris 141-161

Menggambar area tengah papan dengan tulisan "MONOPOLY".

## 4.7 GetShortName()

**Lokasi:** Baris 163-197

Memperpendek nama tile menggunakan abbreviations dictionary.

## 4.8 GetPlayerMarkers()

**Lokasi:** Baris 199-203

```csharp
private string GetPlayerMarkers(List<IPlayer> players)
{
    if (players.Count == 0) return "";
    return " " + string.Join("", players.Select(p => p.Symbol));
}
```

## 4.9 GetSpecialTileInfo()

**Lokasi:** Baris 205-218

```csharp
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
```

## 4.10 ShowPlayerInfo()

**Lokasi:** Baris 220-249

Menampilkan detail pemain: uang, posisi, state, net worth, properties, jail card.

## 4.11 ShowAllPlayersInfo()

**Lokasi:** Baris 251-265

Menampilkan tabel ringkasan semua pemain dengan border box.

## 4.12 ShowMessage/Error/Success/Warning()

**Lokasi:** Baris 267-291

```csharp
public void ShowMessage(string message) => Console.WriteLine(message);

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
```

## 4.13 ShowDiceRoll()

**Lokasi:** Baris 293-307

Menampilkan animasi ASCII dadu dengan hasil dan deteksi doubles.

## 4.14 ShowCard()

**Lokasi:** Baris 309-327

Menampilkan kartu dalam box ASCII dengan word wrap untuk deskripsi.

## 4.15 ShowMenu()

**Lokasi:** Baris 329-341

```csharp
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
```

## 4.16 GetPlayerChoice()

**Lokasi:** Baris 343-353

Validasi input angka dalam range 1 sampai maxOptions.

## 4.17 GetPlayerInput()

**Lokasi:** Baris 355-359

Mendapatkan input string dari user.

## 4.18 GetYesNo()

**Lokasi:** Baris 361-371

Mendapatkan konfirmasi y/n dari user.

## 4.19 ShowPropertyDetails()

**Lokasi:** Baris 373-406

Menampilkan detail properti dengan box ASCII: harga, tipe, house cost, rent table, mortgage value, owner info.

## 4.20 ShowTradeOffer()

**Lokasi:** Baris 408-437

Menampilkan ringkasan trade proposal antara dua pemain.

## 4.21 ShowGameOver()

**Lokasi:** Baris 439-468

Menampilkan ASCII art "GAME OVER" dan info pemenang.

## 4.22 ShowWelcome()

**Lokasi:** Baris 470-498

Menampilkan ASCII art "MONOPOLY" dan "Console Edition - C# Version".

## 4.23 WaitForKeyPress()

**Lokasi:** Baris 500-504

```csharp
public void WaitForKeyPress()
{
    Console.WriteLine("\nPress any key to continue...");
    Console.ReadKey();
}
```

---

# 5. Models

Bagian ini mendokumentasikan semua class Model yang digunakan dalam aplikasi.

---

## 7.1 Player.cs

**Lokasi:** `Models/Player.cs`  
**Total Baris:** 73

### Deskripsi

Class `Player` merepresentasikan pemain dalam permainan Monopoly. Setiap pemain memiliki nama, uang, aset, posisi di papan, dan status permainan.

### Properties

```csharp
public string Name { get; set; }              // Nama pemain
public int RouteIndex { get; set; }           // Posisi di route (0-39)
public PlayerState State { get; set; }        // Normal, InJail, Bankrupt
public IMoney Money { get; }                  // Uang pemain
public List<IAsset> Assets { get; }           // Daftar aset yang dimiliki
public ITile? CurrentTile { get; set; }       // Tile saat ini
public char Symbol { get; }                   // Simbol pemain (1,2,3,4)
public int JailTurns { get; set; }            // Jumlah giliran di jail
public bool HasGetOutOfJailCard { get; set; } // Punya kartu bebas jail
```

### Constructor

```csharp
public Player(string name, int initialMoney = 1500)
{
    Name = name;
    Money = new Money(initialMoney);
    Assets = new List<IAsset>();
    RouteIndex = 0;
    State = PlayerState.Normal;
    CurrentTile = null;
    Symbol = PlayerSymbols[_playerCount % PlayerSymbols.Length];
    _playerCount++;
    JailTurns = 0;
    HasGetOutOfJailCard = false;
}
```

### Methods

#### 7.1.1 AddAsset(IAsset asset)

**Lokasi:** Baris 35-39

```csharp
public void AddAsset(IAsset asset)
{
    asset.Owner = this;
    Assets.Add(asset);
}
```

**Deskripsi:** Menambahkan aset ke pemain dan set owner aset ke pemain ini.

#### 7.1.2 RemoveAsset(IAsset asset)

**Lokasi:** Baris 41-45

```csharp
public void RemoveAsset(IAsset asset)
{
    asset.Owner = null;
    Assets.Remove(asset);
}
```

**Deskripsi:** Menghapus aset dari pemain dan set owner ke null.

#### 7.1.3 GetTotalAssetValue()

**Lokasi:** Baris 47-56

```csharp
public int GetTotalAssetValue()
{
    int total = 0;
    foreach (var asset in Assets)
    {
        total += asset.Value;
        total += asset.AmountHouse * asset.HouseCost;
    }
    return total;
}
```

**Deskripsi:** Menghitung total nilai semua aset termasuk rumah.

#### 7.1.4 GetNetWorth()

**Lokasi:** Baris 58-61

```csharp
public int GetNetWorth()
{
    return Money.Balance + GetTotalAssetValue();
}
```

**Deskripsi:** Menghitung total kekayaan pemain (uang + aset).

#### 7.1.5 ResetPlayerCount() [Static]

**Lokasi:** Baris 63-66

```csharp
public static void ResetPlayerCount()
{
    _playerCount = 0;
}
```

**Deskripsi:** Reset counter pemain untuk game baru.

---

## 7.2 Asset.cs

**Lokasi:** `Models/Asset.cs`  
**Total Baris:** 83

### Deskripsi

Class `Asset` merepresentasikan properti yang bisa dibeli dalam permainan, termasuk real estate, railroad, dan utility.

### Properties

```csharp
public string Name { get; set; }                    // Nama properti
public TypeAsset TypeAsset { get; set; }           // REAL_ESTATE, RAILROAD, PUBLIC_SERVICE
public AssetsCondition AssetsCondition { get; set; } // NORMAL, MORTGAGED
public int Value { get; set; }                      // Harga beli
public IPlayer? Owner { get; set; }                 // Pemilik (null = bank)
public int AmountHouse { get; set; }                // Jumlah rumah (0-5)
public int HouseCost { get; }                       // Biaya per rumah
public int[] Rent { get; }                          // Array rent berdasarkan rumah
public int ColorGroup { get; }                      // Group warna (1-8)
```

### Constructor

```csharp
public Asset(string name, TypeAsset typeAsset, int value, int[] rent, 
             int houseCost = 0, int colorGroup = 0)
{
    Name = name;
    TypeAsset = typeAsset;
    Value = value;
    Rent = rent;
    HouseCost = houseCost;
    ColorGroup = colorGroup;
    AssetsCondition = AssetsCondition.NORMAL;
    Owner = null;
    AmountHouse = 0;
}
```

### Methods

#### 7.2.1 CalculateRent(int diceRoll, int sameColorCount)

**Lokasi:** Baris 31-59

```csharp
public int CalculateRent(int diceRoll = 0, int sameColorCount = 0)
{
    if (AssetsCondition == AssetsCondition.MORTGAGED)
        return 0;

    switch (TypeAsset)
    {
        case TypeAsset.REAL_ESTATE:
            int rentIndex = Math.Min(AmountHouse, Rent.Length - 1);
            int baseRent = Rent[rentIndex];
            // Double rent if owner owns all properties in color group and no houses
            if (AmountHouse == 0 && sameColorCount >= GetColorGroupSize())
                return baseRent * 2;
            return baseRent;

        case TypeAsset.RAILROAD:
            int railroadRentIndex = Math.Min(sameColorCount - 1, Rent.Length - 1);
            return railroadRentIndex >= 0 ? Rent[railroadRentIndex] : Rent[0];

        case TypeAsset.PUBLIC_SERVICE:
            int multiplier = sameColorCount >= 2 ? 10 : 4;
            return diceRoll * multiplier;

        default:
            return 0;
    }
}
```

**Penjelasan Logic:**
- **Real Estate:** Rent berdasarkan jumlah rumah, double jika full color group
- **Railroad:** $25, $50, $100, $200 berdasarkan jumlah RR yang dimiliki
- **Utility:** 4× atau 10× dice roll tergantung jumlah utility

#### 7.2.2 GetColorGroupSize() [Private]

**Lokasi:** Baris 61-66

```csharp
private int GetColorGroupSize()
{
    // Color groups with 2 properties: Brown (1), Dark Blue (8)
    return (ColorGroup == 1 || ColorGroup == 8) ? 2 : 3;
}
```

**Deskripsi:** Return jumlah properti dalam color group (2 atau 3).

#### 7.2.3 GetMortgageValue()

**Lokasi:** Baris 68-71

```csharp
public int GetMortgageValue()
{
    return Value / 2;
}
```

**Deskripsi:** Return nilai mortgage (50% dari harga).

#### 7.2.4 GetUnmortgageValue()

**Lokasi:** Baris 73-76

```csharp
public int GetUnmortgageValue()
{
    return (int)(GetMortgageValue() * 1.1);
}
```

**Deskripsi:** Return biaya unmortgage (110% dari mortgage value).

---

## 7.3 Board.cs

**Lokasi:** `Models/Board.cs`  
**Total Baris:** 65

### Deskripsi

Class `Board` merepresentasikan papan permainan Monopoly dengan grid 11×11 dan route 40 tile.

### Properties

```csharp
public int Width { get; }           // Lebar papan (11)
public int Height { get; }          // Tinggi papan (11)
public ITile?[,] Grid { get; }      // Grid 2D untuk rendering
public List<ITile> Route { get; }   // Route 40 tile untuk pergerakan
```

### Constructor

```csharp
public Board(int width, int height)
{
    if (width < 3 || height < 3)
        throw new ArgumentException("Board must be at least 3x3");

    Width = width;
    Height = height;
    Grid = new ITile?[width, height];
    Route = new List<ITile>();
}
```

### Methods

#### 7.3.1 SetTile(int x, int y, ITile tile)

**Lokasi:** Baris 24-31

```csharp
public void SetTile(int x, int y, ITile tile)
{
    if (x < 0 || x >= Width || y < 0 || y >= Height)
        throw new ArgumentOutOfRangeException($"Position ({x}, {y}) is out of bounds");

    tile.Pos = new TilePos(x, y);
    Grid[x, y] = tile;
}
```

**Deskripsi:** Set tile di posisi tertentu pada grid.

#### 7.3.2 AddToRoute(ITile tile)

**Lokasi:** Baris 33-37

```csharp
public void AddToRoute(ITile tile)
{
    tile.PathIndex = Route.Count;
    Route.Add(tile);
}
```

**Deskripsi:** Tambahkan tile ke route dan set PathIndex-nya.

#### 7.3.3 GetTileAt(int x, int y)

**Lokasi:** Baris 39-45

```csharp
public ITile? GetTileAt(int x, int y)
{
    if (x < 0 || x >= Width || y < 0 || y >= Height)
        return null;

    return Grid[x, y];
}
```

**Deskripsi:** Ambil tile di posisi grid tertentu.

#### 7.3.4 GetTileByPathIndex(int pathIndex)

**Lokasi:** Baris 47-58

```csharp
public ITile? GetTileByPathIndex(int pathIndex)
{
    if (pathIndex < 0)
        pathIndex = Route.Count + (pathIndex % Route.Count);
    
    pathIndex = pathIndex % Route.Count;
    
    if (pathIndex >= 0 && pathIndex < Route.Count)
        return Route[pathIndex];

    return null;
}
```

**Deskripsi:** Ambil tile berdasarkan path index (dengan wrap-around).

#### 7.3.5 GetTotalPathLength()

**Lokasi:** Baris 60-63

```csharp
public int GetTotalPathLength()
{
    return Route.Count;
}
```

**Deskripsi:** Return jumlah tile di route (40).

---

## 7.4 Money.cs

**Lokasi:** `Models/Money.cs`  
**Total Baris:** 43

### Deskripsi

Class `Money` mengelola uang pemain dengan validasi untuk mencegah nilai negatif.

### Properties

```csharp
public int Balance { get; private set; }  // Saldo uang saat ini
```

### Constructor

```csharp
public Money(int initialBalance = 1500)
{
    if (initialBalance < 0)
        throw new ArgumentException("Initial balance cannot be negative");
    
    Balance = initialBalance;
}
```

### Methods

#### 7.4.1 Add(int amount)

**Lokasi:** Baris 17-23

```csharp
public void Add(int amount)
{
    if (amount < 0)
        throw new ArgumentException("Amount cannot be negative");
    
    Balance += amount;
}
```

**Deskripsi:** Tambah uang ke saldo.

#### 7.4.2 Subtract(int amount)

**Lokasi:** Baris 25-36

```csharp
public bool Subtract(int amount)
{
    if (amount < 0)
        throw new ArgumentException("Amount cannot be negative");
    
    if (Balance >= amount)
    {
        Balance -= amount;
        return true;
    }
    return false;
}
```

**Deskripsi:** Kurangi uang dari saldo. Return `true` jika berhasil, `false` jika tidak cukup.

---

## 7.5 Tile.cs

**Lokasi:** `Models/Tile.cs`  
**Total Baris:** 34

### Deskripsi

Class `Tile` merepresentasikan satu tile di papan permainan.

### Properties

```csharp
public string Name { get; set; }          // Nama tile
public TilePos Pos { get; set; }          // Posisi (x, y) di grid
public int? PathIndex { get; set; }       // Index di route (0-39)
public char Display { get; set; }         // Karakter display
public TilesType Type { get; set; }       // PROPERTY, CORNER, SPECIAL, dll
public EffectType EffectType { get; set; } // GO, TAX, CHANCE, dll
public IAsset? Asset { get; set; }        // Aset properti (jika ada)
```

### Constructor

```csharp
public Tile(TilePos pos, string name, char display = ' ', 
            TilesType type = TilesType.SPECIAL, 
            EffectType effectType = EffectType.NOTHING, 
            int? pathIndex = null)
{
    Pos = pos;
    Name = name;
    Display = display;
    Type = type;
    EffectType = effectType;
    PathIndex = pathIndex;
    Asset = null;
}
```

---

## 7.6 Card.cs

**Lokasi:** `Models/Card.cs`  
**Total Baris:** 26

### Deskripsi

Class `Card` merepresentasikan kartu Community Chest atau Chance.

### Properties

```csharp
public string Name { get; }            // Nama kartu
public string Description { get; }     // Deskripsi efek
public CardEffect CardEffect { get; }  // Jenis efek (RECEIVE_MONEY, PAY_MONEY, dll)
public int Value { get; }              // Nilai terkait (uang atau posisi)
```

### Constructor

```csharp
public Card(string name, string description, CardEffect cardEffect, int value = 0)
{
    Name = name;
    Description = description;
    CardEffect = cardEffect;
    Value = value;
}
```

---

## 7.7 Decks.cs

**Lokasi:** `Models/Decks.cs`  
**Total Baris:** 47

### Deskripsi

Class `Decks` mengelola kumpulan kartu dengan fitur shuffle dan draw.

### Properties

```csharp
public List<ICard> Cards => _cards;  // Daftar kartu dalam deck
```

### Private Fields

```csharp
private readonly List<ICard> _cards;   // Kartu
private readonly Random _random;        // Random generator
private int _currentIndex;              // Index kartu saat ini
```

### Constructor

```csharp
public Decks(List<ICard> cards)
{
    _cards = cards ?? throw new ArgumentNullException(nameof(cards));
    _random = new Random();
    _currentIndex = 0;
    Shuffle();
}
```

### Methods

#### 7.7.1 DrawCard()

**Lokasi:** Baris 21-33

```csharp
public ICard DrawCard()
{
    if (_cards.Count == 0)
        throw new InvalidOperationException("Deck is empty");

    if (_currentIndex >= _cards.Count)
    {
        Shuffle();
        _currentIndex = 0;
    }

    return _cards[_currentIndex++];
}
```

**Deskripsi:** Ambil kartu dari deck. Jika sudah habis, shuffle ulang.

#### 7.7.2 Shuffle()

**Lokasi:** Baris 35-45

```csharp
public void Shuffle()
{
    int n = _cards.Count;
    while (n > 1)
    {
        n--;
        int k = _random.Next(n + 1);
        (_cards[k], _cards[n]) = (_cards[n], _cards[k]);
    }
    _currentIndex = 0;
}
```

**Deskripsi:** Acak urutan kartu menggunakan Fisher-Yates shuffle.

---

## 7.8 Dice.cs

**Lokasi:** `Models/Dice.cs`  
**Total Baris:** 25

### Deskripsi

Class `Dice` merepresentasikan dadu dengan nilai maksimum yang dapat dikonfigurasi.

### Properties

```csharp
public int Max { get; }  // Nilai maksimum dadu (default: 6)
```

### Private Fields

```csharp
private readonly Random _random;  // Random generator
```

### Constructor

```csharp
public Dice(int max = 6)
{
    if (max < 1)
        throw new ArgumentException("Max value must be at least 1");
    
    Max = max;
    _random = new Random();
}
```

### Methods

#### 7.8.1 Roll()

**Lokasi:** Baris 20-23

```csharp
public int Roll()
{
    return _random.Next(1, Max + 1);
}
```

**Deskripsi:** Roll dadu dan return nilai acak antara 1 dan Max (inklusif).

---

# 6. Flow Permainan

## 7.1 Alur Setup Game

```
┌─────────────────────────────────────────────────────────────┐
│                    GAME INITIALIZATION                       │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
              ┌───────────────────────────────┐
              │  1. Tampilkan Welcome Screen   │
              │     (ShowWelcome)              │
              └───────────────────────────────┘
                              │
                              ▼
              ┌───────────────────────────────┐
              │  2. Input jumlah pemain (2-4) │
              │     (SetupPlayers)            │
              └───────────────────────────────┘
                              │
                              ▼
              ┌───────────────────────────────┐
              │  3. Input nama setiap pemain  │
              └───────────────────────────────┘
                              │
                              ▼
              ┌───────────────────────────────┐
              │  4. Buat Board Standard       │
              │     (BoardPreset)             │
              └───────────────────────────────┘
                              │
                              ▼
              ┌───────────────────────────────┐
              │  5. Buat GameController       │
              │     + Subscribe Events        │
              └───────────────────────────────┘
                              │
                              ▼
              ┌───────────────────────────────┐
              │  6. StartGame()               │
              │     + RunGameLoop()           │
              └───────────────────────────────┘
```

## 7.2 Alur Game Loop Utama

```
┌─────────────────────────────────────────────────────────────┐
│                     MAIN GAME LOOP                           │
└─────────────────────────────────────────────────────────────┘
       │
       ▼
┌──────────────────┐
│ while (!GameOver)│◄──────────────────────────────┐
└──────────────────┘                               │
       │                                            │
       ▼                                            │
┌──────────────────┐     Ya     ┌──────────────────┐│
│ Player Bankrupt? │──────────►│ NextTurn()       ││
└──────────────────┘            └──────────────────┘│
       │ Tidak                                      │
       ▼                                            │
┌──────────────────┐                               │
│ Draw Board       │                               │
│ Show Players     │                               │
└──────────────────┘                               │
       │                                            │
       ▼                                            │
┌──────────────────┐     Ya     ┌──────────────────┐
│ Player InJail?   │──────────►│ HandleJailTurn() │
└──────────────────┘            └──────────────────┘
       │ Tidak                          │
       ▼                                │
┌──────────────────┐            Still   │
│ Show Action Menu │◄───────────Jail ───┘
│ 1. Roll Dice     │
│ 2. View Props    │
│ 3. Manage Props  │
│ 4. Trade         │
│ 5. End Turn      │
└──────────────────┘
       │
       ▼
┌──────────────────┐
│ Process Action   │
└──────────────────┘
       │
       ▼
┌──────────────────┐
│ PostTurnActions()│
│ NextTurn()       │────────────────────────────────┘
└──────────────────┘
```

## 7.3 Alur Giliran Pemain (Player Turn Flow)

1. **Cek Status Pemain**
   - Skip jika `State == Bankrupt`

2. **Tampilkan State**
   - `DrawBoard()` - papan permainan
   - `ShowAllPlayersInfo()` - tabel status
   - `ShowPlayerInfo()` - detail pemain aktif

3. **Handle Jail (jika InJail)**
   - Opsi: Roll doubles, Pay $50, Use card
   - Jika masih di jail, skip giliran

4. **Roll & Move**
   - Roll 2 dadu
   - Cek doubles (roll lagi, atau jail jika 3x)
   - `MovePlayer(total)`
   - `OnLand()` - proses efek tile

5. **Handle Property Purchase**
   - Jika tile properti tanpa owner
   - Tampilkan detail, tanya beli

6. **Post-Turn Actions**
   - Handle saldo negatif
   - Opsi: Sell house, Mortgage, Bankruptcy

## 7.4 Alur Jail System

```
┌─────────────────┐
│ Player in Jail  │
└─────────────────┘
        │
        ▼
┌─────────────────────────────────────────┐
│              JAIL OPTIONS               │
├─────────────────────────────────────────┤
│ 1. Try Roll Doubles                     │
│    - Jika doubles: bebas, lanjut main   │
│    - Jika tidak: tetap di jail          │
│    - Turn ke-3: paksa bayar $50         │
├─────────────────────────────────────────┤
│ 2. Pay $50                              │
│    - Langsung bebas                     │
│    - Lanjut main normal                 │
├─────────────────────────────────────────┤
│ 3. Use Get Out of Jail Card (jika ada) │
│    - Gunakan kartu                      │
│    - Langsung bebas                     │
└─────────────────────────────────────────┘
```

## 7.5 Alur Pembelian Properti

```
┌──────────────────────────────────────────────────┐
│           PROPERTY PURCHASE FLOW                 │
└──────────────────────────────────────────────────┘
                      │
                      ▼
            ┌──────────────────┐
            │ Land on Property │
            └──────────────────┘
                      │
                      ▼
           ┌───────────────────────┐
           │ Apakah ada Owner?     │
           └───────────────────────┘
             │               │
            Ya              Tidak
             │               │
             ▼               ▼
      ┌────────────┐  ┌────────────────────┐
      │ Bayar Rent │  │ ShowPropertyDetails│
      └────────────┘  └────────────────────┘
                              │
                              ▼
                     ┌─────────────────┐
                     │ Cukup uang?     │
                     └─────────────────┘
                       │           │
                      Ya          Tidak
                       │           │
                       ▼           ▼
              ┌────────────┐  ┌──────────────┐
              │ Tanya beli │  │ Show Warning │
              └────────────┘  └──────────────┘
                    │
           ┌───────┴───────┐
          Ya              Tidak
           │               │
           ▼               ▼
    ┌─────────────┐  ┌──────────────┐
    │PlayerBuyAsset│  │ Skip        │
    └─────────────┘  └──────────────┘
```

## 7.6 Alur Pembayaran Rent

```
Rent Calculation:

REAL ESTATE:
├── Base rent (no houses): Rent[0]
│   └── Double jika full color group
├── 1 House: Rent[1]
├── 2 Houses: Rent[2]
├── 3 Houses: Rent[3]
├── 4 Houses: Rent[4]
└── Hotel (5): Rent[5]

RAILROAD:
├── 1 RR owned: $25
├── 2 RR owned: $50
├── 3 RR owned: $100
└── 4 RR owned: $200

UTILITY:
├── 1 Utility: 4 × dice roll
└── 2 Utilities: 10 × dice roll
```

## 7.7 Alur Building Houses/Hotels

```
┌─────────────────────────────────────────────────────────────┐
│                  HOUSE BUILDING RULES                        │
├─────────────────────────────────────────────────────────────┤
│ 1. Harus own SEMUA properti di color group                 │
│ 2. Tidak boleh mortgage                                     │
│ 3. Maximum 5 rumah (4 houses + 1 hotel)                    │
│ 4. Harga sesuai HouseCost properti                         │
│ 5. Even building rule (dalam color group harus merata)     │
└─────────────────────────────────────────────────────────────┘

Color Groups:
┌─────────┬───────────────┬─────────────┐
│ Group   │ Properties    │ House Cost  │
├─────────┼───────────────┼─────────────┤
│ Brown   │ 2             │ $50         │
│ L.Blue  │ 3             │ $50         │
│ Pink    │ 3             │ $100        │
│ Orange  │ 3             │ $100        │
│ Red     │ 3             │ $150        │
│ Yellow  │ 3             │ $150        │
│ Green   │ 3             │ $200        │
│ D.Blue  │ 2             │ $200        │
└─────────┴───────────────┴─────────────┘
```

## 7.8 Alur Mortgage System

```
MORTGAGE:
├── Dapat 50% dari harga properti
├── Tidak bisa collect rent
├── Harus jual semua rumah dulu
└── Properti tetap milik pemain

UNMORTGAGE:
├── Bayar 110% dari mortgage value
│   (Mortgage value × 1.1)
└── Properti kembali normal
```

## 7.9 Alur Trading System

```
┌─────────────────────────────────────────────────────────────┐
│                    TRADING FLOW                              │
└─────────────────────────────────────────────────────────────┘
                         │
                         ▼
             ┌─────────────────────┐
             │ Select Trade Partner│
             └─────────────────────┘
                         │
                         ▼
             ┌─────────────────────┐
             │ Select Your Props   │
             │ Enter Money to Offer│
             └─────────────────────┘
                         │
                         ▼
             ┌─────────────────────┐
             │ Select Their Props  │
             │ Enter Money to Ask  │
             └─────────────────────┘
                         │
                         ▼
             ┌─────────────────────┐
             │ Show Trade Summary  │
             └─────────────────────┘
                         │
                         ▼
             ┌─────────────────────┐
             │ Partner Accepts?    │
             └─────────────────────┘
               │               │
              Yes             No
               │               │
               ▼               ▼
        ┌────────────┐  ┌────────────┐
        │ Execute    │  │ Cancelled  │
        │ Trade      │  │            │
        └────────────┘  └────────────┘
```

## 7.10 Alur Card System

```
COMMUNITY CHEST / CHANCE:
┌─────────────────────────────────────────────────────────────┐
│ 1. DrawCard() dari deck                                     │
│ 2. Tampilkan card (ShowCard)                                │
│ 3. Apply effect berdasarkan CardEffect:                     │
│    ├── RECEIVE_MONEY: Add money                             │
│    ├── PAY_MONEY: Subtract money (cek bankrupt)             │
│    ├── GO_TO_JAIL: SendToJail()                             │
│    ├── GET_OUT_OF_JAIL: Set HasGetOutOfJailCard = true     │
│    └── MOVE: MovePlayer() atau MovePlayerToPosition()       │
│              lalu OnLand() untuk proses tile baru           │
└─────────────────────────────────────────────────────────────┘
```

## 7.11 Alur Bankruptcy

```
┌─────────────────────────────────────────────────────────────┐
│                   BANKRUPTCY CHECK                           │
└─────────────────────────────────────────────────────────────┘
                         │
                         ▼
              ┌─────────────────────┐
              │ Money + Assets < 0? │
              └─────────────────────┘
                │               │
               Ya             Tidak
                │               │
                ▼               ▼
         ┌────────────┐   ┌────────────┐
         │ Selamatkan │   │ Continue   │
         │ (Sell/     │   │ Playing    │
         │ Mortgage)  │   └────────────┘
         └────────────┘
              │
              ▼
      ┌───────────────────┐
      │ Masih negatif?    │
      └───────────────────┘
        │           │
       Ya          Tidak
        │           │
        ▼           ▼
  ┌──────────┐ ┌────────────┐
  │ BANKRUPT │ │ Continue   │
  │          │ │ Playing    │
  │ - State  │ └────────────┘
  │   = Bankrupt           
  │ - Assets               
  │   = dikembalikan       
  │   ke bank              
  └──────────┘
```

## 7.12 Alur Game Over

```
┌─────────────────────────────────────────────────────────────┐
│                     GAME OVER CHECK                          │
└─────────────────────────────────────────────────────────────┘
                         │
                         ▼
              ┌─────────────────────┐
              │ GetActivePlayers()  │
              │ .Count == 1 ?       │
              └─────────────────────┘
                │               │
               Ya             Tidak
                │               │
                ▼               ▼
         ┌────────────┐   ┌────────────┐
         │ GAME OVER! │   │ Continue   │
         │            │   │ Game       │
         │ IsGameOver │   └────────────┘
         │   = true   │
         │            │
         │ Winner =   │
         │ last active│
         │ player     │
         │            │
         │ Show       │
         │ GameOver   │
         │ Screen     │
         └────────────┘
```

---

# 7. Cara Menjalankan Aplikasi

## 7.1 Prerequisites

- .NET 6.0 SDK atau lebih baru
- Terminal/Console yang mendukung UTF-8 encoding
- Terminal yang mendukung ANSI color codes

## 7.2 Build

```bash
# Navigate to project directory
cd ConsoleMonopolyApp

# Restore dependencies
dotnet restore

# Build project
dotnet build
```

## 7.3 Run

```bash
# Run the application
dotnet run
```

## 7.4 Gameplay Instructions

1. **Welcome Screen**: Tekan sembarang tombol untuk mulai
2. **Player Setup**: 
   - Masukkan jumlah pemain (2-4)
   - Masukkan nama setiap pemain
3. **Game Loop**:
   - Pilih aksi dari menu yang tersedia
   - Roll dice untuk bergerak
   - Beli properti, bangun rumah, atau trade
4. **Winning**: 
   - Pemain terakhir yang tidak bankrupt menang

## 7.5 Controls

| Key | Action |
|-----|--------|
| 1-5 | Select menu option |
| y/n | Confirm/Cancel |
| Enter | Submit input |
| Any key | Continue (saat diminta) |

---

## Catatan Pengembang

### Event-Driven Architecture

Komunikasi antara Controller dan View dilakukan melalui events:

```csharp
// Di Program.cs (subscriber)
_game.OnMessage += (msg) => _view.ShowMessage(msg);
_game.OnDiceRolled += (player, d1, d2) => _view.ShowDiceRoll(d1, d2);

// Di GameController.cs (publisher)
OnMessage?.Invoke("Game Started!");
OnDiceRolled?.Invoke(CurrentPlayer, dice1, dice2);
```

### Color Groups

| ID | Color | Properties |
|----|-------|------------|
| 1 | Brown | Mediterranean Ave, Baltic Ave |
| 2 | Light Blue | Oriental Ave, Vermont Ave, Connecticut Ave |
| 3 | Pink | St. Charles Pl, States Ave, Virginia Ave |
| 4 | Orange | St. James Pl, Tennessee Ave, New York Ave |
| 5 | Red | Kentucky Ave, Indiana Ave, Illinois Ave |
| 6 | Yellow | Atlantic Ave, Ventnor Ave, Marvin Gardens |
| 7 | Green | Pacific Ave, N. Carolina Ave, Pennsylvania Ave |
| 8 | Dark Blue | Park Place, Boardwalk |

---

**Dibuat dengan ❤️ menggunakan C# dan .NET**
