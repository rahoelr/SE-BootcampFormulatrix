namespace MonopolyBackend.DTOs.Requests
{
    public class TradeRequest : PlayerActionRequest
    {
        public string TargetPlayerName { get; set; } = string.Empty;
        public List<string> OfferedProperties { get; set; } = new();
        public int OfferedMoney { get; set; }
        public List<string> RequestedProperties { get; set; } = new();
        public int RequestedMoney { get; set; }
    }
}
