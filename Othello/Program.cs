using Othello.Controllers;
using Othello.Enums;
using Othello.Interfaces;
using Othello.Models;
using Othello.Views;

namespace Othello;

class Program
{
    static void Main(string[] args)
    {
        // Set console encoding for Unicode characters
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        
        // Create players
        var players = new List<IPlayer>
        {
            new Player("Player 1", PlayerColor.Black),
            new Player("Player 2", PlayerColor.White)
        };
        
        // Create board (8x8 standard Othello)
        var board = new Board(8);
        
        // Create game controller
        var gameController = new GameController(players, board);
        
        // Create console view and run the game
        var consoleView = new ConsoleView(gameController);
        consoleView.Run();
    }
}
