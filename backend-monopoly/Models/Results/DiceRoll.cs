namespace MonopolyBackend.Models.Results
{
    /// <summary>
    /// Domain model for dice roll result
    /// </summary>
    public class DiceRoll
    {
        public int Dice1 { get; set; }
        public int Dice2 { get; set; }
        public int Total => Dice1 + Dice2;
        public bool IsDouble => Dice1 == Dice2;
    }
}
