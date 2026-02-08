namespace MonopolyBackend.DTOs.Responses
{
    public record ForceEndGameResponse
    {
        public bool Success { get; init; }
        public string Message { get; init; } = string.Empty;
        public GameResultResponse GameResult { get; init; } = new();
    }

    public record GameResultResponse
    {
        public bool IsGameOver { get; init; }
        public string WinnerName { get; init; } = string.Empty;
        public int TotalTurns { get; init; }
        public List<PlayerRankingResponse> Rankings { get; init; } = new();
    }
}
