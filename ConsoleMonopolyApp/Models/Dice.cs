using ConsoleMonopolyApp.Interfaces;

namespace ConsoleMonopolyApp.Models;

public class Dice : IDice
{
    private readonly Random _random;
    
    public int Max { get; }

    public Dice(int max = 6)
    {
        if (max < 1)
            throw new ArgumentException("Max value must be at least 1", nameof(max));
        
        Max = max;
        _random = new Random();
    }

    public int Roll()
    {
        return _random.Next(1, Max + 1);
    }
}
