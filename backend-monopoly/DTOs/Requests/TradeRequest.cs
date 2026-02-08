namespace MonopolyBackend.DTOs.Requests
{
    public record TradeRequest : PlayerActionRequest
    {
        public string TargetPlayerName { get; init; } = string.Empty;
        public List<string> OfferedProperties { get; init; } = new();
        public int OfferedMoney { get; init; }
        public List<string> RequestedProperties { get; init; } = new();
        public int RequestedMoney { get; init; }
    }
}
