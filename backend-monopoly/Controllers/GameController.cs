using Microsoft.AspNetCore.Mvc;
using MonopolyBackend.Services;
using MonopolyBackend.DTOs.Requests;
using MonopolyBackend.DTOs.Responses;

namespace MonopolyBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameController : ControllerBase
    {
        private readonly GameManager _gameManager;

        public GameController(GameManager gameManager)
        {
            _gameManager = gameManager;
        }

        /// <summary>
        /// Create new game with 2-4 players
        /// </summary>
        [HttpPost("create")]
        public ActionResult<GameStateResponse> CreateGame([FromBody] CreateGameRequest request)
        {
            try
            {
                if (request.PlayerNames == null || request.PlayerNames.Count < 2 || request.PlayerNames.Count > 4)
                {
                    return BadRequest(new { error = "Must have 2-4 players" });
                }

                var game = _gameManager.CreateGame(request.PlayerNames);
                var gameState = game.GetGameState();
                
                return Ok(gameState);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Reset current game
        /// </summary>
        [HttpPost("reset")]
        public ActionResult Reset()
        {
            _gameManager.ResetGame();
            return Ok(new { message = "Game reset successfully" });
        }

        /// <summary>
        /// Check if game exists
        /// </summary>
        [HttpGet("status")]
        public ActionResult GetStatus()
        {
            var hasGame = _gameManager.HasActiveGame();
            return Ok(new { hasActiveGame = hasGame });
        }

        /// <summary>
        /// Get current game state (polling endpoint)
        /// </summary>
        [HttpGet("state")]
        public ActionResult<GameStateResponse> GetGameState()
        {
            try
            {
                var game = _gameManager.GetGame();
                if (game == null)
                    return NotFound(new { error = "No active game. Create one first." });

                var gameState = game.GetGameState();
                return Ok(gameState);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("roll-dice")]
        public ActionResult<RollDiceResponse> RollDice([FromBody] PlayerActionRequest request)
        {
            var game = _gameManager.GetGame();
            if (game == null)
                return NotFound(new { error = "No active game" });

            var result = game.ExecuteRollDice(request.PlayerName);
            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error?.Message });

            // MAP: Domain (RollDiceResult) → DTO (RollDiceResponse)
            var dto = new RollDiceResponse
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
            var game = _gameManager.GetGame();
            if (game == null)
                return NotFound(new { error = "No active game" });

            var result = game.ExecuteBuyProperty(request.PlayerName);
            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error?.Message });

            // MAP: Domain (PropertyActionResult) → DTO (ActionResultResponse)
            var dto = new ActionResultResponse
            {
                Success = result.Data.Success,
                Message = result.Data.Message
            };

            return Ok(dto);
        }

        [HttpPost("build-house")]
        public ActionResult<ActionResultResponse> BuildHouse([FromBody] BuildHouseRequest request)
        {
            var game = _gameManager.GetGame();
            if (game == null)
                return NotFound(new { error = "No active game" });

            var result = game.ExecuteBuildHouse(request.PlayerName, request.PropertyName);
            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error?.Message });

            // MAP: Domain (PropertyActionResult) → DTO (ActionResultResponse)
            var dto = new ActionResultResponse
            {
                Success = result.Data.Success,
                Message = result.Data.Message
            };

            return Ok(dto);
        }

        [HttpPost("sell-house")]
        public ActionResult<ActionResultResponse> SellHouse([FromBody] BuildHouseRequest request)
        {
            var game = _gameManager.GetGame();
            if (game == null)
                return NotFound(new { error = "No active game" });

            var result = game.ExecuteSellHouse(request.PlayerName, request.PropertyName);
            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error?.Message });

            // MAP: Domain (PropertyActionResult) → DTO (ActionResultResponse)
            var dto = new ActionResultResponse
            {
                Success = result.Data.Success,
                Message = result.Data.Message
            };

            return Ok(dto);
        }

        [HttpPost("mortgage")]
        public ActionResult<ActionResultResponse> Mortgage([FromBody] MortgagePropertyRequest request)
        {
            var game = _gameManager.GetGame();
            if (game == null)
                return NotFound(new { error = "No active game" });

            var result = game.ExecuteMortgage(request.PlayerName, request.PropertyName);
            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error?.Message });

            // MAP: Domain (PropertyActionResult) → DTO (ActionResultResponse)
            var dto = new ActionResultResponse
            {
                Success = result.Data.Success,
                Message = result.Data.Message
            };

            return Ok(dto);
        }

        [HttpPost("unmortgage")]
        public ActionResult<ActionResultResponse> Unmortgage([FromBody] MortgagePropertyRequest request)
        {
            var game = _gameManager.GetGame();
            if (game == null)
                return NotFound(new { error = "No active game" });

            var result = game.ExecuteUnmortgage(request.PlayerName, request.PropertyName);
            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error?.Message });

            // MAP: Domain (PropertyActionResult) → DTO (ActionResultResponse)
            var dto = new ActionResultResponse
            {
                Success = result.Data.Success,
                Message = result.Data.Message
            };

            return Ok(dto);
        }

        [HttpPost("trade")]
        public ActionResult<ActionResultResponse> Trade([FromBody] TradeRequest request)
        {
            var game = _gameManager.GetGame();
            if (game == null)
                return NotFound(new { error = "No active game" });

            var result = game.ExecuteTrade(request);
            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error?.Message });

            // MAP: Domain (TradeResult) → DTO (ActionResultResponse)
            var dto = new ActionResultResponse
            {
                Success = result.Data.Success,
                Message = result.Data.Message
            };

            return Ok(dto);
        }

        [HttpPost("pay-jail-fee")]
        public ActionResult<ActionResultResponse> PayJailFee([FromBody] PlayerActionRequest request)
        {
            var game = _gameManager.GetGame();
            if (game == null)
                return NotFound(new { error = "No active game" });

            var result = game.ExecutePayJailFee(request.PlayerName);
            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error?.Message });

            // MAP: Domain (bool) → DTO (ActionResultResponse)
            var dto = new ActionResultResponse
            {
                Success = result.Data,
                Message = result.Data ? "Paid jail fee and released" : "Failed to pay jail fee"
            };

            return Ok(dto);
        }

        [HttpPost("use-jail-card")]
        public ActionResult<ActionResultResponse> UseJailCard([FromBody] PlayerActionRequest request)
        {
            var game = _gameManager.GetGame();
            if (game == null)
                return NotFound(new { error = "No active game" });

            var result = game.ExecuteUseJailCard(request.PlayerName);
            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error?.Message });

            // MAP: Domain (bool) → DTO (ActionResultResponse)
            var dto = new ActionResultResponse
            {
                Success = result.Data,
                Message = result.Data ? "Used Get Out of Jail card" : "Failed to use card"
            };

            return Ok(dto);
        }

        [HttpPost("try-roll-doubles")]
        public ActionResult<RollDiceResponse> TryRollDoublesInJail([FromBody] PlayerActionRequest request)
        {
            var game = _gameManager.GetGame();
            if (game == null)
                return NotFound(new { error = "No active game" });

            var result = game.ExecuteTryRollDoublesInJail(request.PlayerName);
            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error?.Message });

            // MAP: Domain (RollDiceResult) → DTO (RollDiceResponse)
            var dto = new RollDiceResponse
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
            var game = _gameManager.GetGame();
            if (game == null)
                return NotFound(new { error = "No active game" });

            var result = game.ExecuteEndTurn(request.PlayerName);
            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error?.Message });

            // MAP: Domain (bool) → DTO (ActionResultResponse)
            var dto = new ActionResultResponse
            {
                Success = true,
                Message = $"Turn ended. Now it's {game.CurrentPlayer.Name}'s turn."
            };

            return Ok(dto);
        }
    }
}
