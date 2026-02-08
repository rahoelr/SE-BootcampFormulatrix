namespace MonopolyBackend.DTOs.Responses
{
    public record BoardResponse
    {
        public List<TileResponse> Tiles { get; init; } = new();

        public int TotalTiles { get; init; }
    }
}
