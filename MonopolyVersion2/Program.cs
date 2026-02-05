using MonopolyApp.Controllers;

public class Program
{
    GameController? _gameController;
    static void Main(string[] args)
    {
        Console.WriteLine("Monopoly Game Started!");
        var _game = new GameController(board, players, dices, chanceDeck);
    }
}