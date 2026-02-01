namespace ConsoleMonopolyApp.Interfaces;

public interface IDecks
{
    List<ICard> Cards { get; }
    ICard DrawCard();
    void Shuffle();
}
