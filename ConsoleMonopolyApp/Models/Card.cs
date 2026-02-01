using ConsoleMonopolyApp.Enums;
using ConsoleMonopolyApp.Interfaces;

namespace ConsoleMonopolyApp.Models;

public class Card : ICard
{
    public string Name { get; }
    public string Description { get; }
    public CardEffect CardEffect { get; }
    public int Value { get; }

    public Card(string name, string description, CardEffect cardEffect, int value = 0)
    {
        Name = name;
        Description = description;
        CardEffect = cardEffect;
        Value = value;
    }

    public override string ToString()
    {
        return $"{Name}: {Description}";
    }
}
