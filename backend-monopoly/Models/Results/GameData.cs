namespace MonopolyBackend.Models.Results
{
    public record GameData
    {
        public bool IsGameOver { get; init; }
        public string? WinnerName { get; init; }
        public int CurrentTurn { get; init; }
        public string CurrentPlayerName { get; init; } = string.Empty;
        public List<PlayerData> Players { get; init; } = new();
        public List<PropertyData> AllProperties { get; init; } = new();
        public List<string> AvailableActions { get; init; } = new();
    }
}
