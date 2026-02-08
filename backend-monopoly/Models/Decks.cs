using MonopolyBackend.Interfaces;

namespace MonopolyBackend.Models
{
    public class Decks : IDecks
    {
        public List<ICard> Cards{ get; set; }

        public Decks(List<ICard> cards)
        {
            Cards = cards;
        }
    }
}