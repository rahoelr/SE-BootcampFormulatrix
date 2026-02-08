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
        // Static field to hold single game instance (no DI, no lock)
        private static GameService? _currentGame;

        [HttpPost("create")]
        public ActionResult<GameStateResponse> CreateGame([FromBody] CreateGameRequest request)
        {
            if (_currentGame != null)
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

            _currentGame = GameInitializationService.CreateGame(request.PlayerNames);
            var gameState = _currentGame.GetGameState();
            
            return Ok(gameState);
        }

        [HttpPost("reset")]
        public ActionResult Reset()
        {
            _currentGame = null;
            return Ok(new { message = "Game reset successfully" });
        }

        [HttpGet("status")]
        public ActionResult GetStatus()
        {
            return Ok(new { hasActiveGame = _currentGame != null });
        }

        [HttpGet("board")]
        public ActionResult<BoardResponse> GetBoardConfiguration()
        {
            var boardConfig = GameInitializationService.GetBoardConfiguration();
            return Ok(boardConfig);
        }

        [HttpGet("state")]
        public ActionResult<GameStateResponse> GetGameState()
        {
            if (_currentGame == null)
                return NotFound(new { error = "No active game. Create one first." });

            var gameState = _currentGame.GetGameState();
            return Ok(gameState);
        }

        [HttpPost("roll-dice")]
        public ActionResult<RollDiceResponse> RollDice([FromBody] PlayerActionRequest request)
        {
            if (_currentGame == null)
                return NotFound(new { error = "No active game" });

            var result = _currentGame.ExecuteRollDice(request.PlayerName);
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
            if (_currentGame == null)
                return NotFound(new { error = "No active game" });

            var result = _currentGame.ExecuteBuyProperty(request.PlayerName);
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
            if (_currentGame == null)
                return NotFound(new { error = "No active game" });

            var result = _currentGame.ExecuteBuildHouse(request.PlayerName, request.PropertyName);
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
            if (_currentGame == null)
                return NotFound(new { error = "No active game" });

            var result = _currentGame.ExecuteSellHouse(request.PlayerName, request.PropertyName);
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
            if (_currentGame == null)
                return NotFound(new { error = "No active game" });

            var result = _currentGame.ExecuteMortgage(request.PlayerName, request.PropertyName);
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
            if (_currentGame == null)
                return NotFound(new { error = "No active game" });

            var result = _currentGame.ExecuteUnmortgage(request.PlayerName, request.PropertyName);
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
            if (_currentGame == null)
                return NotFound(new { error = "No active game" });

            var result = _currentGame.ExecuteTrade(request);
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
            if (_currentGame == null)
                return NotFound(new { error = "No active game" });

            var result = _currentGame.ExecutePayJailFee(request.PlayerName);
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
            if (_currentGame == null)
                return NotFound(new { error = "No active game" });

            var result = _currentGame.ExecuteUseJailCard(request.PlayerName);
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
            if (_currentGame == null)
                return NotFound(new { error = "No active game" });

            var result = _currentGame.ExecuteTryRollDoublesInJail(request.PlayerName);
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
            if (_currentGame == null)
                return NotFound(new { error = "No active game" });

            var result = _currentGame.ExecuteEndTurn(request.PlayerName);
            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error?.Message });

            // MAP: Domain (bool) → DTO (ActionResultResponse)
            var dto = new ActionResultResponse
            {
                Success = true,
                Message = $"Turn ended. Now it's {_currentGame.CurrentPlayer.Name}'s turn."
            };

            return Ok(dto);
        }
    }
}
