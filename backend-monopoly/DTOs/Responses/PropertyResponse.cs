namespace MonopolyBackend.DTOs.Responses
{
    public class PropertyResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int Value { get; set; }
        public string? OwnerName { get; set; }
        public int Houses { get; set; }
        public bool IsMortgaged { get; set; }
        public int Rent { get; set; }
    }
}
