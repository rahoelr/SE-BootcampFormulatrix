namespace MonopolyBackend.DTOs.Responses
{
    public class TileResponse
    {
        public int Position { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public string Effect { get; set; } = string.Empty;

        public int? Price { get; set; }

        public string? AssetType { get; set; }
    }
}
