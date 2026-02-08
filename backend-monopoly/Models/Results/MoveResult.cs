namespace MonopolyBackend.Models.Results
{
    public record MoveResult
    {
        public int NewPosition { get; init; }
        public string TileName { get; init; } = string.Empty;
        public string TileType { get; init; } = string.Empty;
    }
}
