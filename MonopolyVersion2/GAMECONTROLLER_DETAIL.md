# 🎮 Dokumentasi Detail Logic GameController.cs

## Daftar Isi
1. [Inisialisasi & Constructor](#1-inisialisasi--constructor)
2. [Game Loop - PlayTurn()](#2-game-loop---playturn)
3. [Sistem Dadu](#3-sistem-dadu)
4. [Sistem Pergerakan](#4-sistem-pergerakan)
5. [Sistem Landing (OnLand)](#5-sistem-landing-onland)
6. [Sistem Penjara](#6-sistem-penjara)
7. [Sistem Uang](#7-sistem-uang)
8. [Sistem Properti](#8-sistem-properti)
9. [Sistem Sewa](#9-sistem-sewa)
10. [Sistem Kartu](#10-sistem-kartu)
11. [Sistem Trading](#11-sistem-trading)
12. [Sistem Kebangkrutan](#12-sistem-kebangkrutan)

---

## 1. Inisialisasi & Constructor

### Constructor Logic

```csharp
public GameController(IBoard board, List<IPlayer> players, List<IDice> dices, 
                      IDecks communityChestDeck, IDecks chanceDeck, IView view)
```

**Alur Inisialisasi:**

```
┌─────────────────────────────────────────────────────────────┐
│                    CONSTRUCTOR                              │
├─────────────────────────────────────────────────────────────┤
│ 1. VALIDASI                                                 │
│    if (players.Count < 2 || players.Count > 4)             │
│        throw ArgumentException                              │
│                                                             │
│ 2. ASSIGN PROPERTIES                                        │
│    Board = board                                            │
│    Players = players                                        │
│    Dices = dices                                            │
│    ChanceDeck = chanceDeck                                  │
│    CommunityChestDeck = communityChestDeck                  │
│    CurrentTurn = 0                                          │
│    IsGameOver = false                                       │
│    Winner = null                                            │
│                                                             │
│ 3. INISIALISASI DICTIONARY                                  │
│    _playerGetOutOfJailCards[player] = 0  // untuk semua    │
│    _playerJailTurns = new Dictionary<>                      │
│    PlayerAssets[player] = new List<>     // untuk semua    │
│    PlayerMoney[player] = new List<>      // untuk semua    │
│                                                             │
│ 4. SETUP UANG AWAL                                          │
│    foreach player:                                          │
│        if (player.Money.Balance > 0)                        │
│            PlayerMoney[player].Add(new Money(balance))      │
│        player.PathIndex = 0                                 │
│        player.CurrentTile = Board.Path[0]                   │
│                                                             │
│ 5. SETUP TILE ASSETS                                        │
│    foreach tile in Board.Path:                              │
│        if (tile is Property/Railroad/Utility)              │
│            Create Asset and map to TileAssets[tile]         │
│        else                                                 │
│            TileAssets[tile] = null                          │
│                                                             │
│ 6. SUBSCRIBE EVENTS                                         │
│    OnMessage += _view.ShowMessage                           │
│    OnDiceRolled += _view.ShowDiceRoll                       │
│    OnCardDrawn += _view.ShowCard                            │
│    OnPlayerBankrupt += _view.ShowWarning                    │
│    OnPlayerWins += _view.ShowGameOver                       │
└─────────────────────────────────────────────────────────────┘
```

**Penjelasan Detail:**

1. **Validasi Pemain**: Memastikan 2-4 pemain valid
2. **Dictionary PlayerMoney**: Menyimpan uang sebagai List<IMoney> untuk fleksibilitas (bisa pecahan)
3. **TileAssets Mapping**: Membuat objek Asset untuk setiap tile yang bisa dibeli

---

## 2. Game Loop - PlayTurn()

### Diagram Alur PlayTurn

```
┌──────────────────────────────────────────────────────────────────┐
│                         PlayTurn()                               │
└──────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌──────────────────────────────────────────────────────────────────┐
│ STEP 1: CEK PEMAIN BANGKRUT                                      │
│                                                                  │
│ if (currentPlayer.PlayerState == PlayerState.Bankrupt)          │
│ {                                                                │
│     NextTurn();  // Skip pemain ini                              │
│     return;                                                      │
│ }                                                                │
└──────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌──────────────────────────────────────────────────────────────────┐
│ STEP 2: TAMPILKAN STATE GAME                                     │
│                                                                  │
│ _view.ClearScreen();                                             │
│ _view.DrawBoard(Board, Players);                                 │
│ _view.ShowAllPlayersInfo(Players, playerMoneyDict);             │
│ _view.ShowPlayerInfo(currentPlayer, GetPlayerMoney(player));    │
│ _view.ShowTurnHeader(currentPlayer.Name);                       │
└──────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌──────────────────────────────────────────────────────────────────┐
│ STEP 3: CEK PENJARA                                              │
│                                                                  │
│ if (currentPlayer.PlayerState == PlayerState.InJail)            │
│ {                                                                │
│     HandleJailOptions();                                         │
│     if (masih di penjara)                                        │
│     {                                                            │
│         NextTurn();                                              │
│         return;                                                  │
│     }                                                            │
│ }                                                                │
└──────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌──────────────────────────────────────────────────────────────────┐
│ STEP 4: MAIN ACTION LOOP                                         │
│                                                                  │
│ Variables:                                                       │
│   rolled = false           // Sudah lempar dadu?                │
│   canRollAgain = false     // Dapat ganda?                      │
│   consecutiveDoubles = 0   // Counter ganda berturut            │
│                                                                  │
│ do {                                                             │
│     TAMPILKAN MENU:                                              │
│     [1] Lempar Dadu                                              │
│     [2] Lihat Properti                                           │
│     [3] Kelola Properti                                          │
│     [4] Berdagang                                                │
│     [5] Akhiri Giliran                                           │
│                                                                  │
│     switch (choice):                                             │
│         case 1: HandleDiceRoll()                                 │
│         case 2: ShowPlayerProperties()                           │
│         case 3: ManagePlayerProperties()                         │
│         case 4: TradeFlow()                                      │
│         case 5: EndTurn                                          │
│                                                                  │
│ } while (canRollAgain && !InJail && !IsGameOver)                │
└──────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌──────────────────────────────────────────────────────────────────┐
│ STEP 5: POST-TURN ACTIONS                                        │
│                                                                  │
│ if (!IsGameOver)                                                 │
│ {                                                                │
│     HandleNegativeBalance();  // Cek & handle saldo negatif     │
│     _view.WaitForKeyPress();                                     │
│     NextTurn();                                                  │
│ }                                                                │
└──────────────────────────────────────────────────────────────────┘
```

### Logic Lempar Dadu dalam PlayTurn

```csharp
case 1: // Lempar Dadu
    (dice1, dice2) = RollDices();
    rolled = true;

    // CEK GANDA
    if (dice1 == dice2)
    {
        consecutiveDoubles++;
        
        // 3x GANDA = MASUK PENJARA
        if (consecutiveDoubles >= 3)
        {
            SendToJail();
            canRollAgain = false;
        }
        else
        {
            canRollAgain = true;  // Boleh lempar lagi
        }
    }
    else
    {
        canRollAgain = false;
    }

    // JIKA TIDAK DI PENJARA, BERGERAK
    if (currentPlayer.PlayerState != PlayerState.InJail)
    {
        MovePlayer(dice1 + dice2);
        OnLand();              // Eksekusi efek tile
        OfferPropertyPurchase(); // Tawarkan beli jika bisa
    }
```

---

## 3. Sistem Dadu

### RollDices()

```csharp
public (int dice1, int dice2) RollDices()
{
    Random rand = new Random();
    int dice1 = rand.Next(1, 7);  // 1-6
    int dice2 = rand.Next(1, 7);  // 1-6
    
    OnDiceRolled?.Invoke(CurrentPlayer, dice1, dice2);
    OnMessage?.Invoke($"{CurrentPlayer.Name} mendapat {dice1} dan {dice2}");
    
    return (dice1, dice2);
}
```

**Logic:**
- Generate 2 angka random 1-6
- Trigger event untuk view
- Return tuple (dice1, dice2)

---

## 4. Sistem Pergerakan

### MovePlayer(int steps)

```
┌─────────────────────────────────────────────────────────────┐
│                    MovePlayer(steps)                        │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  oldPosition = CurrentPlayer.PathIndex                      │
│  newPosition = (oldPosition + steps) % 40                   │
│                                                             │
│  // CEK MELEWATI GO                                         │
│  if (newPosition < oldPosition && steps > 0)               │
│  {                                                          │
│      AddMoney(CurrentPlayer, 200);  // Bonus GO             │
│      OnMessage("Melewati MULAI, terima $200!")              │
│  }                                                          │
│                                                             │
│  // UPDATE POSISI                                           │
│  CurrentPlayer.PathIndex = newPosition                      │
│  CurrentPlayer.CurrentTile = Board.Path[newPosition]        │
│                                                             │
│  // TRIGGER EVENTS                                          │
│  OnPlayerMoved?.Invoke(CurrentPlayer, CurrentTile)          │
│  OnMessage($"Mendarat di {CurrentTile.Name}")               │
│                                                             │
│  return CurrentPlayer.CurrentTile                           │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

**Contoh:**
- Posisi awal: 35
- Langkah: 8
- Posisi baru: (35 + 8) % 40 = 3
- Karena 3 < 35 → Melewati GO → +$200

### MovePlayerToPosition(int position)

```csharp
// Digunakan oleh kartu "Maju ke..."
public void MovePlayerToPosition(int position)
{
    int oldPosition = CurrentPlayer.PathIndex;

    // Cek melewati GO
    if (position < oldPosition)
    {
        AddMoney(CurrentPlayer, GO_SALARY);
    }

    CurrentPlayer.PathIndex = position;
    CurrentPlayer.CurrentTile = Board.Path[position];
    OnPlayerMoved?.Invoke(CurrentPlayer, CurrentPlayer.CurrentTile);
}
```

---

## 5. Sistem Landing (OnLand)

### Diagram OnLand()

```
┌─────────────────────────────────────────────────────────────┐
│                        OnLand()                             │
└─────────────────────────────────────────────────────────────┘
                         │
              ┌──────────┴──────────┐
              │  tile.EffectType?   │
              └──────────┬──────────┘
                         │
    ┌────────┬────────┬──┴──┬────────┬────────┬────────┐
    ▼        ▼        ▼     ▼        ▼        ▼        ▼
   GO      Tax    GoToJail Chance Community FreeParking Nothing
    │        │        │      │      Chest       │         │
    │        │        │      │        │         │         │
    ▼        ▼        ▼      ▼        ▼         ▼         ▼
 ShowMsg  PayTax  SendTo   Draw    Draw     ShowMsg   Handle
          $200    Jail()   Card    Card              Property
          atau             from    from               Tile
          $100           Chance  Community
                          Deck    Deck
```

### Detail Switch Case

```csharp
public void OnLand()
{
    var tile = CurrentPlayer.CurrentTile;
    if (tile == null) return;

    switch (tile.EffectType)
    {
        case EffectType.Go:
            // Cuma info, bonus GO sudah di MovePlayer
            OnMessage?.Invoke($"{CurrentPlayer.Name} berada di MULAI.");
            break;

        case EffectType.CommunityChest:
            GetAndApplyDeck(CommunityChestDeck);
            break;

        case EffectType.Chance:
            GetAndApplyDeck(ChanceDeck);
            break;

        case EffectType.Tax:
            // Pajak Penghasilan = $200, Pajak Mewah = $100
            int taxAmount = tile.Name.Contains("Mewah") ? 100 : 200;
            if (!SubtractMoney(CurrentPlayer, taxAmount))
            {
                CheckIsBankrupt(CurrentPlayer);
            }
            break;

        case EffectType.GoToJail:
            SendToJail();
            break;

        case EffectType.FreeParking:
            OnMessage?.Invoke($"{CurrentPlayer.Name} parkir gratis.");
            break;

        case EffectType.Nothing:
            // Ini untuk tile properti
            HandlePropertyTile(tile);
            break;
    }
}
```

### HandlePropertyTile(ITile tile)

```
┌─────────────────────────────────────────────────────────────┐
│                  HandlePropertyTile()                       │
└─────────────────────────────────────────────────────────────┘
                         │
                         ▼
              ┌──────────────────────┐
              │  Cek asset ada?      │
              │  TileAssets[tile]    │
              └──────────┬───────────┘
                         │
         ┌───────────────┼───────────────┐
         ▼               ▼               ▼
    asset.Owner     asset.Owner     asset.Owner
       == null      == other         == self
         │              │               │
         ▼              ▼               ▼
   "Tersedia untuk   Bayar sewa     "Anda pemilik
    dibeli $X"       ke owner       properti ini"
                         │
                         ▼
              ┌────────────────────┐
              │ Cek AssetCondition │
              └─────────┬──────────┘
                        │
            ┌───────────┴───────────┐
            ▼                       ▼
       Mortgage                  Normal
            │                       │
            ▼                       ▼
    "Di-mortgage,              CalculateRent()
     tidak ada sewa"           SubtractMoney(rent)
                               AddMoney(owner, rent)
```

---

## 6. Sistem Penjara

### HandleJailTurn()

```csharp
public bool HandleJailTurn()
{
    if (CurrentPlayer.PlayerState != PlayerState.InJail)
        return false;

    // Tambah counter giliran di penjara
    if (!_playerJailTurns.ContainsKey(CurrentPlayer))
        _playerJailTurns[CurrentPlayer] = 0;

    _playerJailTurns[CurrentPlayer]++;
    int jailTurns = _playerJailTurns[CurrentPlayer];

    // Setelah 3 giliran, HARUS bayar
    if (jailTurns >= 3)
    {
        OnMessage?.Invoke("Sudah 3 giliran di penjara, harus bayar $50");
        return PayJailFee();  // Paksa bayar
    }

    return true;  // Masih bisa pilih opsi
}
```

### Diagram Opsi Penjara

```
┌─────────────────────────────────────────────────────────────┐
│               HandleJailOptions()                           │
└─────────────────────────────────────────────────────────────┘
                         │
                         ▼
              ┌──────────────────────┐
              │ HandleJailTurn()     │
              │ (increment counter)  │
              └──────────┬───────────┘
                         │
         ┌───────────────┴───────────────┐
         │ jailTurns >= 3?               │
         └───────────────┬───────────────┘
                 │               │
                YES             NO
                 │               │
                 ▼               ▼
          PayJailFee()    Tampilkan opsi:
          (forced)        [1] Lempar ganda
                          [2] Bayar $50
                          [3] Kartu bebas (jika ada)
                                 │
                    ┌────────────┼────────────┐
                    ▼            ▼            ▼
             TryRoll        PayJail      UseCard
             Doubles         Fee()        ()
                    │            │            │
                    ▼            ▼            ▼
              Dapat ganda?   -$50 dari    Kartu -= 1
                    │        PlayerMoney  State = Normal
            ┌───────┴───────┐    │        JailTurns = 0
            ▼               ▼    │
           YES             NO    │
            │               │    │
            ▼               ▼    ▼
      State=Normal    "Tetap    State = Normal
      JailTurns=0     penjara"  JailTurns = 0
      MovePlayer()
      OnLand()
```

### TryRollDoublesInJail()

```csharp
public bool TryRollDoublesInJail()
{
    if (CurrentPlayer.PlayerState != PlayerState.InJail)
        return false;

    var (dice1, dice2) = RollDices();

    if (dice1 == dice2)  // DAPAT GANDA!
    {
        CurrentPlayer.PlayerState = PlayerState.Normal;
        _playerJailTurns[CurrentPlayer] = 0;
        OnMessage?.Invoke("Dapat ganda, bebas dari penjara!");

        // Langsung bergerak
        MovePlayer(dice1 + dice2);
        OnLand();
        return true;
    }
    else
    {
        OnMessage?.Invoke("Tidak dapat ganda. Tetap di penjara.");
        return false;
    }
}
```

---

## 7. Sistem Uang

### Struktur Penyimpanan Uang

```
PlayerMoney = Dictionary<IPlayer, List<IMoney>>

Contoh:
PlayerMoney[Andi] = [
    Money(1000),  // Satu pecahan $1000
    Money(300),   // Satu pecahan $300
    Money(200)    // Satu pecahan $200
]
Total = $1500
```

### AddMoney(IPlayer player, int amount)

```csharp
public bool AddMoney(IPlayer player, int amount)
{
    if (amount <= 0)
        return false;

    var money = new Money(amount);
    PlayerMoney[player].Add(money);  // Tambah pecahan baru
    
    OnMessage?.Invoke($"{player.Name} menerima ${amount}");
    return true;
}
```

### SubtractMoney(IPlayer player, int amount)

```
┌─────────────────────────────────────────────────────────────┐
│              SubtractMoney(player, amount)                  │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  1. VALIDASI                                                │
│     if (amount <= 0) return false                           │
│                                                             │
│  2. CEK TOTAL UANG                                          │
│     currentMoney = PlayerMoney[player].Sum(m => m.Balance)  │
│     if (currentMoney < amount)                              │
│         return false  // Uang tidak cukup                  │
│                                                             │
│  3. KURANGI PECAHAN (dari terbesar)                         │
│     remaining = amount                                      │
│     moneyList = PlayerMoney[player]                         │
│                   .OrderByDescending(m => m.Balance)        │
│                                                             │
│     foreach (money in moneyList):                           │
│         if (remaining <= 0) break                           │
│                                                             │
│         if (money.Balance <= remaining)                     │
│             // Pecahan habis dipakai                        │
│             remaining -= money.Balance                      │
│             PlayerMoney[player].Remove(money)               │
│         else                                                │
│             // Pecahan cukup, kurangi saja                  │
│             money.Balance -= remaining                      │
│             remaining = 0                                   │
│                                                             │
│  4. BROADCAST                                               │
│     OnMessage($"{player.Name} membayar ${amount}")          │
│     return true                                             │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

**Contoh:**
```
Sebelum: PlayerMoney[Andi] = [Money(1000), Money(300), Money(200)]
SubtractMoney(Andi, 450)

Proses:
- Sorted: [1000, 300, 200]
- remaining = 450
- Money(1000): 1000 > 450, jadi 1000-450=550, remaining=0

Sesudah: PlayerMoney[Andi] = [Money(550), Money(300), Money(200)]
Total: $1050
```

### GetPlayerMoney(IPlayer player)

```csharp
public int GetPlayerMoney(IPlayer player)
{
    return PlayerMoney[player].Sum(m => m.Balance);
}
```

---

## 8. Sistem Properti

### PlayerBuyAsset(IAsset asset)

```
┌─────────────────────────────────────────────────────────────┐
│                 PlayerBuyAsset(asset)                       │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  1. VALIDASI OWNER                                          │
│     if (asset.Owner != null)                                │
│         return false  // Sudah ada pemilik                 │
│                                                             │
│  2. KURANGI UANG                                            │
│     if (!SubtractMoney(CurrentPlayer, asset.Value))        │
│         return false  // Uang tidak cukup                  │
│                                                             │
│  3. SET OWNERSHIP                                           │
│     asset.Owner = CurrentPlayer                             │
│     CurrentPlayer.Assets.Add(asset)                         │
│     PlayerAssets[CurrentPlayer].Add(asset)                  │
│                                                             │
│  4. BROADCAST                                               │
│     OnPropertyBought?.Invoke(CurrentPlayer, asset)          │
│     OnMessage($"Membeli {asset.Name} seharga ${asset}")     │
│                                                             │
│     return true                                             │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### PlayerAddHouse(IAsset asset)

```csharp
public bool PlayerAddHouse(IAsset asset)
{
    // VALIDASI 1: Kepemilikan
    if (asset.Owner != CurrentPlayer)
        return false;

    // VALIDASI 2: Tipe aset
    if (asset.TypeAsset != TypeAsset.RealEstate)
        return false;  // Hanya RealEstate bisa bangun rumah

    // VALIDASI 3: Maksimum rumah
    if (asset.AmountHouse >= 5)
        return false;  // Sudah hotel (max)

    // HITUNG BIAYA
    int houseCost = asset.Value / 2;  // 50% dari harga properti

    // KURANGI UANG
    if (!SubtractMoney(CurrentPlayer, houseCost))
        return false;

    // TAMBAH RUMAH
    asset.AmountHouse++;
    
    string buildingType = asset.AmountHouse == 5 ? "hotel" : "rumah";
    OnMessage?.Invoke($"Membangun {buildingType} di {asset.Name}");
    
    return true;
}
```

### PlayerMortgageAsset & PlayerUnmortgageAsset

```
┌─────────────────────────────────────────────────────────────┐
│                    MORTGAGE FLOW                            │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  MORTGAGE:                                                  │
│  ─────────                                                  │
│  1. Cek ownership                                           │
│  2. Cek belum mortgage                                      │
│  3. Cek tidak ada rumah (AmountHouse == 0)                  │
│  4. Set AssetCondition = Mortgage                           │
│  5. AddMoney(player, Value/2)  // 50% dari harga           │
│                                                             │
│  UNMORTGAGE:                                                │
│  ───────────                                                │
│  1. Cek ownership                                           │
│  2. Cek sudah mortgage                                      │
│  3. Hitung biaya: (Value/2) * 1.1  // 110% dari mortgage   │
│  4. SubtractMoney(player, cost)                             │
│  5. Set AssetCondition = Normal                             │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## 9. Sistem Sewa

### CalculateRent(IAsset asset)

```
┌─────────────────────────────────────────────────────────────┐
│                   CalculateRent(asset)                      │
└─────────────────────────────────────────────────────────────┘
                         │
              ┌──────────┴──────────┐
              │  asset.TypeAsset?   │
              └──────────┬──────────┘
                         │
         ┌───────────────┼───────────────┐
         ▼               ▼               ▼
   PublicService     Railroad       RealEstate
    (Utility)                            │
         │               │               │
         ▼               ▼               ▼
    Count same      Count same     Base = Value/10
    type owned      type owned          │
         │               │               │
         ▼               ▼       ┌───────┴───────┐
    1 utility: $25   1: $25      │ AmountHouse?  │
    2 utility: $50   2: $50      └───────┬───────┘
                     3: $100             │
                     4: $200     ┌───┬───┼───┬───┬───┐
                                 0   1   2   3   4   5
                                 │   │   │   │   │   │
                                 ▼   ▼   ▼   ▼   ▼   ▼
                               base  5x  15x 45x 80x 100x
```

### Kode CalculateRent

```csharp
private int CalculateRent(IAsset asset)
{
    int sameTypeCount = CountSameTypeAssets(asset.Owner!, asset);

    // UTILITY
    if (asset.TypeAsset == TypeAsset.PublicService)
    {
        return sameTypeCount == 2 ? 50 : 25;
    }

    // RAILROAD
    if (asset.TypeAsset == TypeAsset.Railroad)
    {
        return sameTypeCount switch
        {
            1 => 25,
            2 => 50,
            3 => 100,
            4 => 200,
            _ => 25
        };
    }

    // REALESTATE
    int baseRent = asset.Value / 10;  // 10% dari harga

    if (asset.AmountHouse > 0)
    {
        return asset.AmountHouse switch
        {
            1 => baseRent * 5,
            2 => baseRent * 15,
            3 => baseRent * 45,
            4 => baseRent * 80,
            5 => baseRent * 100,  // Hotel
            _ => baseRent
        };
    }

    return baseRent;
}
```

---

## 10. Sistem Kartu

### DrawCardFromDeck(IDecks deck)

```csharp
public ICard DrawCardFromDeck(IDecks deck)
{
    var cards = deck.Cards;
    if (cards == null || cards.Count == 0)
        throw new InvalidOperationException("Deck kosong");

    // Ambil kartu pertama
    var card = cards[0];
    
    // Pindahkan ke belakang (circular shuffle)
    cards.RemoveAt(0);
    cards.Add(card);

    OnCardDrawn?.Invoke(card);
    OnMessage?.Invoke($"Kartu: {card.Name} - {card.Description}");

    return card;
}
```

### ApplyCardEffect(ICard card)

```
┌─────────────────────────────────────────────────────────────┐
│                  ApplyCardEffect(card)                      │
└─────────────────────────────────────────────────────────────┘
                         │
              ┌──────────┴──────────┐
              │  card.CardEffect?   │
              └──────────┬──────────┘
                         │
    ┌────────┬────────┬──┴──┬────────┬────────┐
    ▼        ▼        ▼     ▼        ▼        
 Receive   Pay     GoTo   GetOut    Move
 Money    Money    Jail    Jail       │
    │        │       │       │        │
    ▼        ▼       ▼       ▼        ▼
 AddMoney Subtract SendTo  Card++   if(value<0)
 (value)  Money    Jail()          MovePlayer
          (value)                    else
             │                    MoveToPos
             ▼                       │
          Gagal?                     ▼
             │                    OnLand()
             ▼
        Bankrupt?
```

---

## 11. Sistem Trading

### TradeFlow() - Diagram Lengkap

```
┌─────────────────────────────────────────────────────────────┐
│                      TradeFlow()                            │
└─────────────────────────────────────────────────────────────┘
                         │
                         ▼
              ┌──────────────────────┐
              │ Get other players    │
              │ (not bankrupt)       │
              └──────────┬───────────┘
                         │
                         ▼
              ┌──────────────────────┐
              │ Select target player │
              └──────────┬───────────┘
                         │
                         ▼
              ┌──────────────────────┐
              │ Select properties    │
              │ to OFFER             │
              └──────────┬───────────┘
                         │
                         ▼
              ┌──────────────────────┐
              │ Input money to OFFER │
              └──────────┬───────────┘
                         │
                         ▼
              ┌──────────────────────┐
              │ Select properties    │
              │ to REQUEST           │
              └──────────┬───────────┘
                         │
                         ▼
              ┌──────────────────────┐
              │ Input money REQUEST  │
              └──────────┬───────────┘
                         │
                         ▼
              ┌──────────────────────┐
              │ ShowTradeOffer()     │
              │ (summary)            │
              └──────────┬───────────┘
                         │
                         ▼
              ┌──────────────────────┐
              │ Target accepts?      │
              └──────────┬───────────┘
                  │            │
                 YES          NO
                  │            │
                  ▼            ▼
          PlayerProposeTrade  "Ditolak"
```

### PlayerProposeTrade Logic

```csharp
public bool PlayerProposeTrade(IPlayer player1, IPlayer player2,
                        List<IAsset> offer1, int money1,
                        List<IAsset> offer2, int money2)
{
    // 1. VALIDASI KEPEMILIKAN PROPERTI
    foreach (var asset in offer1)
        if (asset.Owner != player1) return false;
    
    foreach (var asset in offer2)
        if (asset.Owner != player2) return false;

    // 2. VALIDASI UANG CUKUP
    if (GetPlayerMoney(player1) < money1) return false;
    if (GetPlayerMoney(player2) < money2) return false;

    // 3. TRANSFER PROPERTI player1 → player2
    foreach (var asset in offer1)
    {
        asset.Owner = player2;
        player1.Assets.Remove(asset);
        PlayerAssets[player1].Remove(asset);
        player2.Assets.Add(asset);
        PlayerAssets[player2].Add(asset);
    }

    // 4. TRANSFER PROPERTI player2 → player1
    foreach (var asset in offer2)
    {
        asset.Owner = player1;
        player2.Assets.Remove(asset);
        PlayerAssets[player2].Remove(asset);
        player1.Assets.Add(asset);
        PlayerAssets[player1].Add(asset);
    }

    // 5. TRANSFER UANG
    if (money1 > 0)
    {
        SubtractMoney(player1, money1);
        AddMoney(player2, money1);
    }
    if (money2 > 0)
    {
        SubtractMoney(player2, money2);
        AddMoney(player1, money2);
    }

    OnMessage?.Invoke("Perdagangan selesai!");
    return true;
}
```

---

## 12. Sistem Kebangkrutan

### CheckIsBankrupt(IPlayer player)

```
┌─────────────────────────────────────────────────────────────┐
│               CheckIsBankrupt(player)                       │
└─────────────────────────────────────────────────────────────┘
                         │
                         ▼
              ┌──────────────────────┐
              │ Hitung total value:  │
              │ - Uang tunai         │
              │ - Nilai semua aset   │
              └──────────┬───────────┘
                         │
                         ▼
              ┌──────────────────────┐
              │ total < 0 ?          │
              └──────────┬───────────┘
                  │            │
                 YES          NO
                  │            │
                  ▼            ▼
              BANGKRUT      return false
                  │
                  ▼
         ┌────────────────────┐
         │ 1. Set Bankrupt    │
         │ 2. Return assets   │
         │    to bank         │
         │ 3. Clear player    │
         │    assets list     │
         │ 4. Check winner    │
         └────────────────────┘
                  │
                  ▼
         ┌────────────────────┐
         │ Only 1 player left?│
         └─────────┬──────────┘
              │         │
             YES       NO
              │         │
              ▼         ▼
         IsGameOver   return
         = true       true
         Winner = 
         last player
```

### HandleNegativeBalance()

```csharp
private void HandleNegativeBalance()
{
    var currentPlayer = CurrentPlayer;
    int playerMoney = GetPlayerMoney(currentPlayer);

    if (playerMoney < 0)
    {
        _view.ShowWarning("Saldo negatif!");

        // Loop sampai saldo positif atau bangkrut
        while (GetPlayerMoney(currentPlayer) < 0 
               && currentPlayer.Assets.Count > 0)
        {
            _view.ShowMenu("Kumpulkan dana!", new List<string>
            {
                "Jual Rumah",
                "Mortgage Properti",
                "Nyatakan Bangkrut"
            });

            int choice = _view.GetPlayerChoice(3);
            switch (choice)
            {
                case 1: SellHouseFlow(); break;
                case 2: MortgageFlow(); break;
                case 3: CheckIsBankrupt(currentPlayer); return;
            }
        }

        // Masih negatif setelah semua usaha
        if (GetPlayerMoney(currentPlayer) < 0)
        {
            CheckIsBankrupt(currentPlayer);
        }
    }
}
```

---

## Ringkasan Flow Keseluruhan

```
┌──────────────────────────────────────────────────────────────────┐
│                    MONOPOLY GAME FLOW                            │
├──────────────────────────────────────────────────────────────────┤
│                                                                  │
│  INIT:  Constructor → Setup Board → Setup Players → Setup Decks │
│                              │                                   │
│                              ▼                                   │
│  START: StartGame() → Show welcome message                       │
│                              │                                   │
│                              ▼                                   │
│  LOOP:  ┌─────────────────────────────────────────────────────┐ │
│         │                PlayTurn()                            │ │
│         │  1. Check bankrupt → skip if true                   │ │
│         │  2. Display game state                              │ │
│         │  3. Handle jail if InJail                           │ │
│         │  4. Show action menu                                │ │
│         │  5. Process dice roll → move → land                 │ │
│         │  6. Handle property purchase                        │ │
│         │  7. Handle negative balance                         │ │
│         │  8. NextTurn()                                      │ │
│         └─────────────────────────────────────────────────────┘ │
│                              │                                   │
│                              ▼                                   │
│  END:   IsGameOver == true → Show winner → Exit                 │
│                                                                  │
└──────────────────────────────────────────────────────────────────┘
```

---

*Dokumentasi detail logic GameController.cs - Proyek Monopoly Console C#*
