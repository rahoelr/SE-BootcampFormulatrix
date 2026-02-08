using MonopolyBackend.Enums;
using MonopolyBackend.Interfaces;

namespace MonopolyBackend.Models
{
    public class Card : ICard
    {
        public string Name {get; set;}
        public CardEffect CardEffect {get; set;}
        public string? Description {get; set;}
        public int Value {get; set;}
        public Card(string name, string description, CardEffect cardEffect, int value)
        {
            Name = name;
            Description = description;
            CardEffect = cardEffect;
            Value = value;
        }
    }
}