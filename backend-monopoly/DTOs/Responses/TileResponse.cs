namespace MonopolyBackend.DTOs.Responses
{
    public record TileResponse
    {
        public int Position { get; init; }

        public string Name { get; init; } = string.Empty;

        public string Type { get; init; } = string.Empty;

        public string Effect { get; init; } = string.Empty;

        public int? Price { get; init; }

        public string? AssetType { get; init; }
    }
}
