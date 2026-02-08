using MonopolyBackend.Interfaces;

namespace MonopolyBackend.Models
{
    public class Dice : IDice
    {
        public int Max {get; set;}

        public Dice(int max)
        {
            Max = max;
        }
    }
}