using ConsoleMonopolyApp.Interfaces;

namespace ConsoleMonopolyApp.Models;

public class Money : IMoney
{
    public int Balance { get; private set; }

    public Money(int initialBalance = 1500)
    {
        if (initialBalance < 0)
            throw new ArgumentException("Initial balance cannot be negative", nameof(initialBalance));
        
        Balance = initialBalance;
    }

    public void Add(int amount)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));
        
        Balance += amount;
    }

    public bool Subtract(int amount)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));
        
        if (Balance >= amount)
        {
            Balance -= amount;
            return true;
        }
        return false;
    }

    public override string ToString()
    {
        return $"${Balance}";
    }
}
