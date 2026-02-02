using MonopolyApp.Interfaces;

namespace MonopolyApp.Models
{
    public class Money : IMoney
    {
        public int Balance { get; set; }

        public Money(int balance)
        {
            Balance = balance;
        }

        public void Add(int amount)
        {
            Balance += amount;
        }

        public void Subtract(int amount)
        {
            Balance -= amount;
        }
    }
}