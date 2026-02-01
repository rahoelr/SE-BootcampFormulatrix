using ConsoleMonopolyApp.Interfaces;

namespace ConsoleMonopolyApp.Models;

public class Decks : IDecks
{
    private readonly List<ICard> _cards;
    private readonly Random _random;
    private int _currentIndex;

    public List<ICard> Cards => _cards;

    public Decks(List<ICard> cards)
    {
        _cards = cards ?? throw new ArgumentNullException(nameof(cards));
        _random = new Random();
        _currentIndex = 0;
        Shuffle();
    }

    public ICard DrawCard()
    {
        if (_cards.Count == 0)
            throw new InvalidOperationException("Deck is empty");

        if (_currentIndex >= _cards.Count)
        {
            Shuffle();
            _currentIndex = 0;
        }

        return _cards[_currentIndex++];
    }

    public void Shuffle()
    {
        int n = _cards.Count;
        while (n > 1)
        {
            n--;
            int k = _random.Next(n + 1);
            (_cards[k], _cards[n]) = (_cards[n], _cards[k]);
        }
        _currentIndex = 0;
    }
}
