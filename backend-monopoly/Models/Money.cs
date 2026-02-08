using MonopolyBackend.Interfaces;

namespace MonopolyBackend.Models
{
    public class Money : IMoney
    {
        public int Balance { get; set; }

        public Money(int initialBalance)
        {
            Balance = initialBalance;
        }
    }
}