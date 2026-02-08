namespace MonopolyBackend.Models.Results
{
    /// <summary>
    /// Domain model for player movement result
    /// </summary>
    public class MoveResult
    {
        public int NewPosition { get; set; }
        public string TileName { get; set; } = string.Empty;
        public string TileType { get; set; } = string.Empty;
    }
}
