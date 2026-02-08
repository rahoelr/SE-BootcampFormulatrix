namespace MonopolyBackend.DTOs.Responses
{
    public class GameStateResponse
    {
        public bool IsGameStarted { get; set; }
        public bool IsGameOver { get; set; }
        public string? WinnerName { get; set; }
        public int CurrentTurn { get; set; }
        public string CurrentPlayerName { get; set; } = string.Empty;
        public List<string> AvailableActions { get; set; } = new();
        public List<PlayerResponse> Players { get; set; } = new();
        public List<PropertyResponse> AllProperties { get; set; } = new();
    }
}
