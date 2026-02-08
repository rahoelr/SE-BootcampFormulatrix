# GameService Refactoring Guide

## Current Status
- ✅ Infrastructure complete (DTOs, GameManager, GameInitializationService, Controller, Program.cs)
- ✅ Event collection system added (BeginAction, AddActionMessage, GetCurrentActionMessages)
- ⚠️ 62 compilation errors from `_view` dependencies

## Methods to DELETE (Console-only, incompatible with REST API)

All these methods use `_view` and are console interactive flows:

1. **PlayTurn()** - Lines 103-223
   - Console game loop with blocking input
   - DELETE entire method

2. **TradeFlow()** - Lines 225-299  
   - DELETE entire method
   
3. **HandleNegativeBalance()** - Lines 320-362
   - DELETE entire method

4. **HandleJailOptions()** - Lines 456-499
   - DELETE entire method

5. **OfferPropertyPurchase()** - Lines 630-656
   - DELETE entire method

6. **ShowPlayerProperties()** - Lines 686-712
   - DELETE entire method

7. **ManagePlayerProperties()** - Lines 715-754
   - DELETE entire method

8. **BuildHouseFlow()** - Lines 757-788
   - DELETE entire method

9. **SellHouseFlow()** - Lines 833-861
   - DELETE entire method

10. **MortgageFlow()** - Lines 989-1017
    - DELETE entire method

11. **UnmortgageFlow()** - Lines 1054-1082
    - DELETE entire method

## Replace All OnMessage?.Invoke() calls

Find and replace throughout file:
```csharp
// Before:
OnMessage?.Invoke("message");

// After:
AddActionMessage("message");
```

**Locations:** ~50+ occurrences throughout the file

## Add Missing Using Statement

At top of file, add:
```csharp
using MonopolyBackend.DTOs.Responses;
using MonopolyBackend.Enums;
```

## Add DTO Mapper Methods

Add this region after line 362 (after GetPlayerMoney method):

```csharp
#region DTO Mappers

public GameStateResponse GetGameState()
{
    var response = new GameStateResponse
    {
        IsGameStarted = Players.Count > 0,
        IsGameOver = IsGameOver,
        WinnerName = Winner?.Name,
        CurrentTurn = CurrentTurn,
        CurrentPlayerName = CurrentPlayer?.Name ?? "",
        Players = Players.Select(MapPlayerToResponse).ToList(),
        AllProperties = TileAssets
            .Where(kv => kv.Value != null)
            .Select(kv => MapPropertyToResponse(kv.Value!))
            .ToList(),
        AvailableActions = GetAvailableActionsForCurrentPlayer()
    };

    return response;
}

private PlayerResponse MapPlayerToResponse(IPlayer player)
{
    var jailTurns = GetJailTurns(player).Data;
    var hasJailCard = HasGetOutOfJailCard(player).Data;
    var money = GetPlayerMoney(player).Data;
    var currentTile = player.CurrentTile;
    var tileName = currentTile?.Name ?? "Unknown";
    var tileType = currentTile?.TilesType.ToString() ?? "Unknown";

    return new PlayerResponse
    {
        Name = player.Name,
        Position = player.PathIndex,
        CurrentTileName = tileName,
        CurrentTileType = tileType,
        Money = money,
        State = player.PlayerState.ToString(),
        Properties = player.Assets.Select(MapPropertyToResponse).ToList(),
        JailTurns = jailTurns,
        HasGetOutOfJailCard = hasJailCard
    };
}

private PropertyResponse MapPropertyToResponse(IAsset asset)
{
    var rent = asset.Owner != null ? CalculateRent(asset).Data : 0;

    return new PropertyResponse
    {
        Name = asset.Name,
        Type = asset.TypeAsset.ToString(),
        Value = asset.Value,
        OwnerName = asset.Owner?.Name,
        Houses = asset.AmountHouse,
        IsMortgaged = asset.AssetCondition == AssetCondition.Mortgage,
        Rent = rent
    };
}

private List<string> GetAvailableActionsForCurrentPlayer()
{
    var actions = new List<string>();
    var player = CurrentPlayer;

    if (player == null || IsGameOver)
        return actions;

    if (player.PlayerState == PlayerState.Bankrupt)
        return actions;

    if (player.PlayerState == PlayerState.InJail)
    {
        actions.Add("try-roll-doubles");
        actions.Add("pay-jail-fee");
        
        if (HasGetOutOfJailCard(player).Data)
            actions.Add("use-jail-card");
        
        return actions;
    }

    // Normal turn
    actions.Add("roll-dice");
    
    // Can buy property if on unowned property
    var tile = player.CurrentTile;
    if (tile != null && TileAssets.ContainsKey(tile))
    {
        var asset = TileAssets[tile];
        if (asset != null && asset.Owner == null)
        {
            var money = GetPlayerMoney(player).Data;
            if (money >= asset.Value)
                actions.Add("buy-property");
        }
    }

    // Can manage properties if owns any
    if (player.Assets.Any())
    {
        actions.Add("build-house");
        actions.Add("sell-house");
        actions.Add("mortgage");
        actions.Add("unmortgage");
    }

    // Can trade if other players exist
    if (GetActivePlayers().Count > 1)
        actions.Add("trade");

    actions.Add("end-turn");

    return actions;
}

#endregion
```

## Add Atomic Action Methods

Add this region at end of file (before closing brace):

```csharp
#region API Action Methods

private ServiceResult<bool> ValidatePlayerTurn(string playerName)
{
    if (IsGameOver)
        return ServiceResult<bool>.Fail(
            new ServiceError(ErrorType.Validation, "Game is over"));

    if (CurrentPlayer.Name != playerName)
        return ServiceResult<bool>.Fail(
            new ServiceError(ErrorType.Unauthorized, 
                $"Not your turn. Current player: {CurrentPlayer.Name}"));

    if (CurrentPlayer.PlayerState == PlayerState.Bankrupt)
        return ServiceResult<bool>.Fail(
            new ServiceError(ErrorType.Validation, "Player is bankrupt"));

    return ServiceResult<bool>.Success(true);
}

public ServiceResult<RollDiceResponse> ExecuteRollDice(string playerName)
{
    BeginAction();
    
    var validation = ValidatePlayerTurn(playerName);
    if (!validation.IsSuccess)
        return ServiceResult<RollDiceResponse>.Fail(validation.Error!);

    var rollResult = RollDices();
    if (!rollResult.IsSuccess || rollResult.Data == null)
        return ServiceResult<RollDiceResponse>.Fail(
            new ServiceError(ErrorType.Unexpected, "Failed to roll dice"));

    var diceData = rollResult.Data;
    int total = diceData.Dice1 + diceData.Dice2;
    bool isDouble = diceData.Dice1 == diceData.Dice2;

    var moveResult = MovePlayer(total);
    if (!moveResult.IsSuccess)
        return ServiceResult<RollDiceResponse>.Fail(moveResult.Error!);

    OnLand();

    var response = new RollDiceResponse
    {
        Dice1 = diceData.Dice1,
        Dice2 = diceData.Dice2,
        Total = total,
        IsDouble = isDouble,
        NewPosition = CurrentPlayer.PathIndex,
        LandedTile = CurrentPlayer.CurrentTile?.Name ?? "",
        Events = GetCurrentActionMessages()
    };

    return ServiceResult<RollDiceResponse>.Success(response);
}

public ServiceResult<ActionResultResponse> ExecuteBuyProperty(string playerName)
{
    BeginAction();
    
    var validation = ValidatePlayerTurn(playerName);
    if (!validation.IsSuccess)
        return ServiceResult<ActionResultResponse>.Fail(validation.Error!);

    var tile = CurrentPlayer.CurrentTile;
    if (tile == null || !TileAssets.ContainsKey(tile))
        return ServiceResult<ActionResultResponse>.Fail(
            new ServiceError(ErrorType.Validation, "No property at current position"));

    var asset = TileAssets[tile];
    if (asset == null)
        return ServiceResult<ActionResultResponse>.Fail(
            new ServiceError(ErrorType.Validation, "No purchasable asset here"));

    var buyResult = PlayerBuyAsset(asset);
    if (!buyResult.IsSuccess)
        return ServiceResult<ActionResultResponse>.Fail(buyResult.Error!);

    return ServiceResult<ActionResultResponse>.Success(
        new ActionResultResponse
        {
            Success = true,
            Message = $"Successfully bought {asset.Name}",
            Events = GetCurrentActionMessages()
        });
}

public ServiceResult<ActionResultResponse> ExecuteEndTurn(string playerName)
{
    BeginAction();
    
    var validation = ValidatePlayerTurn(playerName);
    if (!validation.IsSuccess)
        return ServiceResult<ActionResultResponse>.Fail(validation.Error!);

    NextTurn();

    return ServiceResult<ActionResultResponse>.Success(
        new ActionResultResponse
        {
            Success = true,
            Message = $"Turn ended. Now {CurrentPlayer.Name}'s turn",
            Events = GetCurrentActionMessages()
        });
}

// TODO: Add remaining action methods:
// - ExecuteBuildHouse
// - ExecuteSellHouse
// - ExecuteMortgage
// - ExecuteUnmortgage
// - ExecuteTrade
// - ExecutePayJailFee
// - ExecuteUseJailCard
// - ExecuteTryRollDoublesInJail

#endregion
```

## Fix Specific Issues

1. **Line 121** - Fix type conversion:
```csharp
// Before:
playerMoneyDict[player] = GetPlayerMoney(player);

// After:
playerMoneyDict[player] = GetPlayerMoney(player).Data;
```

2. **Line 125** - Same fix:
```csharp
// Before:
_view.ShowPlayerInfo(currentPlayer, GetPlayerMoney(currentPlayer));

// After (but delete this entire method anyway):
// This is in PlayTurn which will be deleted
```

3. **Line 164** - Fix deconstruct error:
```csharp
// Before:
(dice1, dice2) = RollDices();

// After (but delete this entire method anyway):
var rollResult = RollDices();
if (rollResult.IsSuccess && rollResult.Data != null)
{
    dice1 = rollResult.Data.Dice1;
    dice2 = rollResult.Data.Dice2;
}
```

## Execution Order

1. DELETE all 11 methods listed above
2. REPLACE all OnMessage?.Invoke with AddActionMessage
3. ADD using statements
4. ADD DTO Mapper methods
5. ADD Atomic Action methods
6. Run `dotnet build` to verify

## Expected Result

After all changes:
- ✅ 0 compilation errors
- ✅ Clean separation: business logic only
- ✅ Ready for API consumption
- ✅ Frontend can poll /api/game/state
- ✅ Frontend can call atomic actions

## Estimated Time

Manual execution: 45-60 minutes
