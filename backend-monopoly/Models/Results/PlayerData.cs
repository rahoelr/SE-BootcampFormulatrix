namespace MonopolyBackend.Models.Results
{
    public record PlayerData
    {
        public string Name { get; init; } = string.Empty;
        public int Position { get; init; }
        public string CurrentTileName { get; init; } = string.Empty;
        public string CurrentTileType { get; init; } = string.Empty;
        public int Money { get; init; }
        public string State { get; init; } = string.Empty;
        public List<PropertyData> Properties { get; init; } = new();
        public int JailTurns { get; init; }
        public bool HasGetOutOfJailCard { get; init; }
    }
}
