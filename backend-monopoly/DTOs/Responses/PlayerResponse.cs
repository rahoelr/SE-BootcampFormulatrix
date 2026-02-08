namespace MonopolyBackend.DTOs.Responses
{
    public class PlayerResponse
    {
        public string Name { get; set; } = string.Empty;
        public int Position { get; set; }
        public string CurrentTileName { get; set; } = string.Empty;
        public string CurrentTileType { get; set; } = string.Empty;
        public int Money { get; set; }
        public string State { get; set; } = "Normal";
        public List<PropertyResponse> Properties { get; set; } = new();
        public int JailTurns { get; set; }
        public bool HasGetOutOfJailCard { get; set; }
    }
}
