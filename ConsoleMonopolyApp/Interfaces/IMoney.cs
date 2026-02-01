namespace ConsoleMonopolyApp.Interfaces;

public interface IMoney
{
    int Balance { get; }
    void Add(int amount);
    bool Subtract(int amount);
}
