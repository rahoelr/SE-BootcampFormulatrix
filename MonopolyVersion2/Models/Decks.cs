using MonopolyApp.Interfaces;

namespace MonopolyApp.Models
{
    public class Deck : IDecks
    {
        public List<ICard> Cards{ get; set; }

        public Deck(List<ICard> cards)
        {
            Cards = cards;
        }
    }
}