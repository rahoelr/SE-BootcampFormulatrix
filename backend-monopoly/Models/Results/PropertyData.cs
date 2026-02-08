namespace MonopolyBackend.Models.Results
{
    public record PropertyData
    {
        public string Name { get; init; } = string.Empty;
        public int Price { get; init; }
        public string Type { get; init; } = string.Empty;
        public bool IsMortgaged { get; init; }
        public int Houses { get; init; }
    }
}
