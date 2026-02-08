namespace MonopolyBackend.Models.Results
{
    public record RollDiceResult
    {
        public DiceRoll Roll { get; init; } = new();
        public MoveResult Move { get; init; } = new();
    }
}
