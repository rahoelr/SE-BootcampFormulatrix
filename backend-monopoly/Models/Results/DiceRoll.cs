namespace MonopolyBackend.Models.Results
{
    public record DiceRoll
    {
        public int Dice1 { get; init; }
        public int Dice2 { get; init; }
        public int Total => Dice1 + Dice2;
        public bool IsDouble => Dice1 == Dice2;
    }
}
