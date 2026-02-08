namespace MonopolyBackend.Models.Results
{
    /// <summary>
    /// Wrapper for roll dice action result (multiple data types)
    /// </summary>
    public class RollDiceResult
    {
        public DiceRoll Roll { get; set; } = new();
        public MoveResult Move { get; set; } = new();
    }
}
