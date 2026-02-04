using MonopolyApp.Interfaces;

namespace MonopolyApp.Models
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