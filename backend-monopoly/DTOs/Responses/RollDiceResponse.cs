namespace MonopolyBackend.DTOs.Responses
{
    public record RollDiceResponse
    {
        public int Dice1 { get; init; }
        public int Dice2 { get; init; }
        public int Total { get; init; }
        public bool IsDouble { get; init; }
        public int NewPosition { get; init; }
        public string LandedTile { get; init; } = string.Empty;
    }
}
