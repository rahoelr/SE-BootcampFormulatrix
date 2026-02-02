using MonopolyApp.Enums;
using MonopolyApp.Interfaces;

namespace MonopolyApp.Models
{
    public class Card : ICard
    {
        public string Name {get; set;}
        public CardEffect CardEffect {get; set;}

        public Card(string name, CardEffect cardEffect)
        {
            Name = name;
            CardEffect = cardEffect;
        }
    }
}