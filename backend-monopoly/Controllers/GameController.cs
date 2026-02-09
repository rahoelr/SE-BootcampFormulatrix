using Microsoft.AspNetCore.Mvc;
using MonopolyBackend.Services;
using MonopolyBackend.DTOs.Requests;
using MonopolyBackend.DTOs.Responses;
using MonopolyBackend.Common;
using MonopolyBackend.Models.Results;

namespace MonopolyBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameController : ControllerBase
    {
        private readonly GameServiceManager _gameManager;

        public GameController(GameServiceManager gameManager)
        {
            _gameManager = gameManager;
        }

        [HttpPost("create")]
        public ActionResult<GameStateResponse> CreateGame([FromBody] CreateGameRequest request)
        {
            if (_gameManager.CurrentGame != null)
            {
                return Conflict(new { error = "Game already exists. Reset first." });
            }

            if (request.PlayerNames == null || request.PlayerNames.Count < 2 || request.PlayerNames.Count > 4)
            {
                return BadRequest(new { error = "Must have 2-4 players" });
            }

            if (request.PlayerNames.Distinct().Count() != request.PlayerNames.Count)
            {
                return BadRequest(new { error = "Player names must be unique" });
            }

            _gameManager.CurrentGame = GameInitializationService.CreateGame(request.PlayerNames);
            ServiceResult<GameData> gameStateResult = _gameManager.CurrentGame.GetGameState();
            
            if (!gameStateResult.IsSuccess)
            {
                return StatusCode(500, new { error = "Failed to create game state" });
            }

            return Ok(gameStateResult.Data);
        }

        [HttpPost("reset")]
        public ActionResult<ResetResponse> Reset()
        {
            _gameManager.Reset();
            ResetResponse response = new ResetResponse
            {
                Success = true,
                Message = "Game reset successfully"
            };
            return Ok(response);
        }

        [HttpGet("status")]
        public ActionResult<GameStatusResponse> GetStatus()
        {
            GameStatusResponse response = new GameStatusResponse
            {
                HasActiveGame = _gameManager.HasActiveGame
            };
            return Ok(response);
        }

        [HttpGet("board")]
        public ActionResult<BoardResponse> GetBoardConfiguration()
        {
            BoardResponse boardConfig = GameInitializationService.GetBoardConfiguration();
            return Ok(boardConfig);
        }

        [HttpGet("state")]
        public ActionResult<GameStateResponse> GetGameState()
        {
            if (_gameManager.CurrentGame == null)
                return NotFound(new { error = "No active game. Create one first." });

            ServiceResult<GameData> gameStateResult = _gameManager.CurrentGame.GetGameState();
            
            if (!gameStateResult.IsSuccess)
            {
                return StatusCode(500, new { error = "Failed to get game state" });
            }

            GameData gameState = gameStateResult.Data;

            GameStateResponse dto = new GameStateResponse
            {
                IsGameStarted = true,
                IsGameOver = gameState.IsGameOver,
                WinnerName = gameState.WinnerName,
                CurrentTurn = gameState.CurrentTurn,
                CurrentPlayerName = gameState.CurrentPlayerName,
                Players = gameState.Players.Select(p => new PlayerResponse
                {
                    Name = p.Name,
                    Position = p.Position,
                    CurrentTileName = p.CurrentTileName,
                    CurrentTileType = p.CurrentTileType,
                    Money = p.Money,
                    State = p.State,
                    Properties = p.Properties.Select(prop => new PropertyResponse
                    {
                        Name = prop.Name,
                        Value = prop.Price,
                        Type = prop.Type,
                        IsMortgaged = prop.IsMortgaged,
                        Houses = prop.Houses,
                        OwnerName = p.Name,
                        Rent = 0
                    }).ToList(),
                    JailTurns = p.JailTurns,
                    HasGetOutOfJailCard = p.HasGetOutOfJailCard
                }).ToList(),
                AllProperties = gameState.AllProperties.Select(prop => new PropertyResponse
                {
                    Name = prop.Name,
                    Value = prop.Price,
                    Type = prop.Type,
                    IsMortgaged = prop.IsMortgaged,
                    Houses = prop.Houses,
                    OwnerName = null,
                    Rent = 0
                }).ToList(),
                AvailableActions = gameState.AvailableActions
            };

            return Ok(dto);
        }

        [HttpPost("roll-dice")]
        public ActionResult<RollDiceResponse> RollDice([FromBody] PlayerActionRequest request)
        {
            if (_gameManager.CurrentGame == null)
                return NotFound(new { error = "No active game" });

            ServiceResult<RollDiceResult> result = _gameManager.CurrentGame.ExecuteRollDice(request.PlayerName);
            if (!result.IsSuccess)
            {
                return result.Error?.Type switch
                {
                    ErrorType.NotFound => NotFound(new { error = result.Error.Message }),
                    ErrorType.Conflict => Conflict(new { error = result.Error.Message }),
                    ErrorType.Validation => BadRequest(new { error = result.Error.Message }),
                    ErrorType.Unauthorized => Unauthorized(new { error = result.Error.Message }),
                    ErrorType.Unexpected => StatusCode(500, new { error = result.Error.Message }),
                    _ => BadRequest(new { error = result.Error?.Message })
                };
            }

            RollDiceResponse dto = new RollDiceResponse
            {
                Dice1 = result.Data.Roll.Dice1,
                Dice2 = result.Data.Roll.Dice2,
                Total = result.Data.Roll.Total,
                IsDouble = result.Data.Roll.IsDouble,
                NewPosition = result.Data.Move.NewPosition,
                LandedTile = result.Data.Move.TileName
            };

            return Ok(dto);
        }

        [HttpPost("buy-property")]
        public ActionResult<ActionResultResponse> BuyProperty([FromBody] PlayerActionRequest request)
        {
            if (_gameManager.CurrentGame == null)
                return NotFound(new { error = "No active game" });

            ServiceResult<PropertyActionResult> result = _gameManager.CurrentGame.ExecuteBuyProperty(request.PlayerName);
            if (!result.IsSuccess)
            {
                return result.Error?.Type switch
                {
                    ErrorType.NotFound => NotFound(new { error = result.Error.Message }),
                    ErrorType.Conflict => Conflict(new { error = result.Error.Message }),
                    ErrorType.Validation => BadRequest(new { error = result.Error.Message }),
                    ErrorType.Unauthorized => Unauthorized(new { error = result.Error.Message }),
                    ErrorType.Unexpected => StatusCode(500, new { error = result.Error.Message }),
                    _ => BadRequest(new { error = result.Error?.Message })
                };
            }

            ActionResultResponse dto = new ActionResultResponse
            {
                Success = result.Data.Success,
                Message = result.Data.Message
            };

            return Ok(dto);
        }

        [HttpPost("build-house")]
        public ActionResult<ActionResultResponse> BuildHouse([FromBody] BuildHouseRequest request)
        {
            if (_gameManager.CurrentGame == null)
                return NotFound(new { error = "No active game" });

            ServiceResult<PropertyActionResult> result = _gameManager.CurrentGame.ExecuteBuildHouse(request.PlayerName, request.PropertyName);
            if (!result.IsSuccess)
            {
                return result.Error?.Type switch
                {
                    ErrorType.NotFound => NotFound(new { error = result.Error.Message }),
                    ErrorType.Conflict => Conflict(new { error = result.Error.Message }),
                    ErrorType.Validation => BadRequest(new { error = result.Error.Message }),
                    ErrorType.Unauthorized => Unauthorized(new { error = result.Error.Message }),
                    ErrorType.Unexpected => StatusCode(500, new { error = result.Error.Message }),
                    _ => BadRequest(new { error = result.Error?.Message })
                };
            }

            ActionResultResponse dto = new ActionResultResponse
            {
                Success = result.Data.Success,
                Message = result.Data.Message
            };

            return Ok(dto);
        }

        [HttpPost("sell-house")]
        public ActionResult<ActionResultResponse> SellHouse([FromBody] BuildHouseRequest request)
        {
            if (_gameManager.CurrentGame == null)
                return NotFound(new { error = "No active game" });

            ServiceResult<PropertyActionResult> result = _gameManager.CurrentGame.ExecuteSellHouse(request.PlayerName, request.PropertyName);
            if (!result.IsSuccess)
            {
                return result.Error?.Type switch
                {
                    ErrorType.NotFound => NotFound(new { error = result.Error.Message }),
                    ErrorType.Conflict => Conflict(new { error = result.Error.Message }),
                    ErrorType.Validation => BadRequest(new { error = result.Error.Message }),
                    ErrorType.Unauthorized => Unauthorized(new { error = result.Error.Message }),
                    ErrorType.Unexpected => StatusCode(500, new { error = result.Error.Message }),
                    _ => BadRequest(new { error = result.Error?.Message })
                };
            }

            ActionResultResponse dto = new ActionResultResponse
            {
                Success = result.Data.Success,
                Message = result.Data.Message
            };

            return Ok(dto);
        }

        [HttpPost("mortgage")]
        public ActionResult<ActionResultResponse> Mortgage([FromBody] MortgagePropertyRequest request)
        {
            if (_gameManager.CurrentGame == null)
                return NotFound(new { error = "No active game" });

            ServiceResult<PropertyActionResult> result = _gameManager.CurrentGame.ExecuteMortgage(request.PlayerName, request.PropertyName);
            if (!result.IsSuccess)
            {
                return result.Error?.Type switch
                {
                    ErrorType.NotFound => NotFound(new { error = result.Error.Message }),
                    ErrorType.Conflict => Conflict(new { error = result.Error.Message }),
                    ErrorType.Validation => BadRequest(new { error = result.Error.Message }),
                    ErrorType.Unauthorized => Unauthorized(new { error = result.Error.Message }),
                    ErrorType.Unexpected => StatusCode(500, new { error = result.Error.Message }),
                    _ => BadRequest(new { error = result.Error?.Message })
                };
            }

            ActionResultResponse dto = new ActionResultResponse
            {
                Success = result.Data.Success,
                Message = result.Data.Message
            };

            return Ok(dto);
        }

        [HttpPost("unmortgage")]
        public ActionResult<ActionResultResponse> Unmortgage([FromBody] MortgagePropertyRequest request)
        {
            if (_gameManager.CurrentGame == null)
                return NotFound(new { error = "No active game" });

            ServiceResult<PropertyActionResult> result = _gameManager.CurrentGame.ExecuteUnmortgage(request.PlayerName, request.PropertyName);
            if (!result.IsSuccess)
            {
                return result.Error?.Type switch
                {
                    ErrorType.NotFound => NotFound(new { error = result.Error.Message }),
                    ErrorType.Conflict => Conflict(new { error = result.Error.Message }),
                    ErrorType.Validation => BadRequest(new { error = result.Error.Message }),
                    ErrorType.Unauthorized => Unauthorized(new { error = result.Error.Message }),
                    ErrorType.Unexpected => StatusCode(500, new { error = result.Error.Message }),
                    _ => BadRequest(new { error = result.Error?.Message })
                };
            }

            ActionResultResponse dto = new ActionResultResponse
            {
                Success = result.Data.Success,
                Message = result.Data.Message
            };

            return Ok(dto);
        }

        [HttpPost("trade")]
        public ActionResult<ActionResultResponse> Trade([FromBody] TradeRequest request)
        {
            if (_gameManager.CurrentGame == null)
                return NotFound(new { error = "No active game" });

            ServiceResult<TradeResult> result = _gameManager.CurrentGame.ExecuteTrade(request);
            if (!result.IsSuccess)
            {
                return result.Error?.Type switch
                {
                    ErrorType.NotFound => NotFound(new { error = result.Error.Message }),
                    ErrorType.Conflict => Conflict(new { error = result.Error.Message }),
                    ErrorType.Validation => BadRequest(new { error = result.Error.Message }),
                    ErrorType.Unauthorized => Unauthorized(new { error = result.Error.Message }),
                    ErrorType.Unexpected => StatusCode(500, new { error = result.Error.Message }),
                    _ => BadRequest(new { error = result.Error?.Message })
                };
            }

            ActionResultResponse dto = new ActionResultResponse
            {
                Success = result.Data.Success,
                Message = result.Data.Message
            };

            return Ok(dto);
        }

        [HttpPost("pay-jail-fee")]
        public ActionResult<ActionResultResponse> PayJailFee([FromBody] PlayerActionRequest request)
        {
            if (_gameManager.CurrentGame == null)
                return NotFound(new { error = "No active game" });

            ServiceResult<bool> result = _gameManager.CurrentGame.ExecutePayJailFee(request.PlayerName);
            if (!result.IsSuccess)
            {
                return result.Error?.Type switch
                {
                    ErrorType.NotFound => NotFound(new { error = result.Error.Message }),
                    ErrorType.Conflict => Conflict(new { error = result.Error.Message }),
                    ErrorType.Validation => BadRequest(new { error = result.Error.Message }),
                    ErrorType.Unauthorized => Unauthorized(new { error = result.Error.Message }),
                    ErrorType.Unexpected => StatusCode(500, new { error = result.Error.Message }),
                    _ => BadRequest(new { error = result.Error?.Message })
                };
            }

            ActionResultResponse dto = new ActionResultResponse
            {
                Success = result.Data,
                Message = result.Data ? "Paid jail fee and released" : "Failed to pay jail fee"
            };

            return Ok(dto);
        }

        [HttpPost("use-jail-card")]
        public ActionResult<ActionResultResponse> UseJailCard([FromBody] PlayerActionRequest request)
        {
            if (_gameManager.CurrentGame == null)
                return NotFound(new { error = "No active game" });

            ServiceResult<bool> result = _gameManager.CurrentGame.ExecuteUseJailCard(request.PlayerName);
            if (!result.IsSuccess)
            {
                return result.Error?.Type switch
                {
                    ErrorType.NotFound => NotFound(new { error = result.Error.Message }),
                    ErrorType.Conflict => Conflict(new { error = result.Error.Message }),
                    ErrorType.Validation => BadRequest(new { error = result.Error.Message }),
                    ErrorType.Unauthorized => Unauthorized(new { error = result.Error.Message }),
                    ErrorType.Unexpected => StatusCode(500, new { error = result.Error.Message }),
                    _ => BadRequest(new { error = result.Error?.Message })
                };
            }

            ActionResultResponse dto = new ActionResultResponse
            {
                Success = result.Data,
                Message = result.Data ? "Used Get Out of Jail card" : "Failed to use card"
            };

            return Ok(dto);
        }

        [HttpPost("try-roll-doubles")]
        public ActionResult<RollDiceResponse> TryRollDoublesInJail([FromBody] PlayerActionRequest request)
        {
            if (_gameManager.CurrentGame == null)
                return NotFound(new { error = "No active game" });

            ServiceResult<RollDiceResult> result = _gameManager.CurrentGame.ExecuteTryRollDoublesInJail(request.PlayerName);
            if (!result.IsSuccess)
            {
                return result.Error?.Type switch
                {
                    ErrorType.NotFound => NotFound(new { error = result.Error.Message }),
                    ErrorType.Conflict => Conflict(new { error = result.Error.Message }),
                    ErrorType.Validation => BadRequest(new { error = result.Error.Message }),
                    ErrorType.Unauthorized => Unauthorized(new { error = result.Error.Message }),
                    ErrorType.Unexpected => StatusCode(500, new { error = result.Error.Message }),
                    _ => BadRequest(new { error = result.Error?.Message })
                };
            }

            RollDiceResponse dto = new RollDiceResponse
            {
                Dice1 = result.Data.Roll.Dice1,
                Dice2 = result.Data.Roll.Dice2,
                Total = result.Data.Roll.Total,
                IsDouble = result.Data.Roll.IsDouble,
                NewPosition = result.Data.Move.NewPosition,
                LandedTile = result.Data.Move.TileName
            };

            return Ok(dto);
        }

        [HttpPost("end-turn")]
        public ActionResult<ActionResultResponse> EndTurn([FromBody] PlayerActionRequest request)
        {
            if (_gameManager.CurrentGame == null)
                return NotFound(new { error = "No active game" });

            ServiceResult<bool> result = _gameManager.CurrentGame.ExecuteEndTurn(request.PlayerName);
            if (!result.IsSuccess)
            {
                return result.Error?.Type switch
                {
                    ErrorType.NotFound => NotFound(new { error = result.Error.Message }),
                    ErrorType.Conflict => Conflict(new { error = result.Error.Message }),
                    ErrorType.Validation => BadRequest(new { error = result.Error.Message }),
                    ErrorType.Unauthorized => Unauthorized(new { error = result.Error.Message }),
                    ErrorType.Unexpected => StatusCode(500, new { error = result.Error.Message }),
                    _ => BadRequest(new { error = result.Error?.Message })
                };
            }

            ActionResultResponse dto = new ActionResultResponse
            {
                Success = true,
                Message = $"Turn ended. Now it's {_gameManager.CurrentGame.CurrentPlayer.Name}'s turn."
            };

            return Ok(dto);
        }

        [HttpPost("force-end")]
        public ActionResult<ForceEndGameResponse> ForceEndGame()
        {
            if (_gameManager.CurrentGame == null)
                return NotFound(new { error = "No active game" });

            ServiceResult<ForceEndGameResult> result = _gameManager.CurrentGame.ExecuteForceEndGame();
            if (!result.IsSuccess)
            {
                return result.Error?.Type switch
                {
                    ErrorType.NotFound => NotFound(new { error = result.Error.Message }),
                    ErrorType.Conflict => Conflict(new { error = result.Error.Message }),
                    ErrorType.Validation => BadRequest(new { error = result.Error.Message }),
                    ErrorType.Unauthorized => Unauthorized(new { error = result.Error.Message }),
                    ErrorType.Unexpected => StatusCode(500, new { error = result.Error.Message }),
                    _ => BadRequest(new { error = result.Error?.Message })
                };
            }

            ForceEndGameResponse dto = new ForceEndGameResponse
            {
                Success = true,
                Message = $"Game ended. Winner: {result.Data.WinnerName}",
                GameResult = new GameResultResponse
                {
                    IsGameOver = result.Data.IsGameOver,
                    WinnerName = result.Data.WinnerName,
                    TotalTurns = result.Data.TotalTurns,
                    Rankings = result.Data.Rankings.Select(r => new PlayerRankingResponse
                    {
                        Rank = r.Rank,
                        PlayerName = r.PlayerName,
                        TotalWealth = r.TotalWealth,
                        Cash = r.Cash,
                        AssetsValue = r.AssetsValue,
                        PropertyCount = r.PropertyCount,
                        HouseCount = r.HouseCount
                    }).ToList()
                }
            };

            return Ok(dto);
        }
    }
}