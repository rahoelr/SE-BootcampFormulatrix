namespace MonopolyBackend.Models.Results
{
    public record ForceEndGameResult
    {
        public bool IsGameOver { get; init; }
        public string WinnerName { get; init; } = string.Empty;
        public int TotalTurns { get; init; }
        public List<PlayerRanking> Rankings { get; init; } = new();
    }

    public class PlayerRanking
    {
        public int Rank { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public int TotalWealth { get; set; }
        public int Cash { get; set; }
        public int AssetsValue { get; set; }
        public int PropertyCount { get; set; }
        public int HouseCount { get; set; }
    }
}
