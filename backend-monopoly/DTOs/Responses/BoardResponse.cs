namespace MonopolyBackend.DTOs.Responses
{
    public class BoardResponse
    {
        public List<TileResponse> Tiles { get; set; } = new();

        public int TotalTiles { get; set; }
    }
}
