namespace MonopolyBackend.Models.Results
{
    /// <summary>
    /// Domain result for trade action between players
    /// </summary>
    public class TradeResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Player1Name { get; set; } = string.Empty;
        public string Player2Name { get; set; } = string.Empty;
    }
}
