namespace MonopolyBackend.DTOs.Responses
{
    public record PlayerRankingResponse
    {
        public int Rank { get; init; }
        public string PlayerName { get; init; } = string.Empty;
        public int TotalWealth { get; init; }
        public int Cash { get; init; }
        public int AssetsValue { get; init; }
        public int PropertyCount { get; init; }
        public int HouseCount { get; init; }
    }
}
