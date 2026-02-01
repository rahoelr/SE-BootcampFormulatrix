using ConsoleMonopolyApp.Enums;

namespace ConsoleMonopolyApp.Interfaces;

public interface ICard
{
    string Name { get; }
    string Description { get; }
    CardEffect CardEffect { get; }
    int Value { get; }
}
