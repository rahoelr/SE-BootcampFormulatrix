namespace MonopolyBackend.DTOs.Responses
{
    public record PropertyResponse
    {
        public string Name { get; init; } = string.Empty;
        public string Type { get; init; } = string.Empty;
        public int Value { get; init; }
        public string? OwnerName { get; init; }
        public int Houses { get; init; }
        public bool IsMortgaged { get; init; }
        public int Rent { get; init; }
    }
}
