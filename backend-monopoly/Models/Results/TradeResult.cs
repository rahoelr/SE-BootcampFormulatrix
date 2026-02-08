namespace MonopolyBackend.Models.Results
{
    public record TradeResult
    {
        public bool Success { get; init; }
        public string Message { get; init; } = string.Empty;
        public string Player1Name { get; init; } = string.Empty;
        public string Player2Name { get; init; } = string.Empty;
    }
}
