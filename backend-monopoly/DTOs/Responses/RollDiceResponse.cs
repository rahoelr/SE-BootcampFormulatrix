namespace MonopolyBackend.DTOs.Responses
{
    public class RollDiceResponse
    {
        public int Dice1 { get; set; }
        public int Dice2 { get; set; }
        public int Total { get; set; }
        public bool IsDouble { get; set; }
        public int NewPosition { get; set; }
        public string LandedTile { get; set; } = string.Empty;
    }
}
