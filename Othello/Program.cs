using Othello.Controllers;
using Othello.Enums;
using Othello.Players;
using Othello.Views;

namespace Othello;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("╔═══════════════════════════════════════╗");
        Console.WriteLine("║           WELCOME TO OTHELLO          ║");
        Console.WriteLine("╚═══════════════════════════════════════╝");
        Console.WriteLine();

        bool playAgain = true;

        while (playAgain)
        {
            var (player1, player2) = SelectGameMode();
            var view = new ConsoleGameView();
            var controller = new GameController(player1, player2, view);

            controller.StartGame();

            playAgain = AskPlayAgain();
        }

        Console.WriteLine("\nThank you for playing Othello! Goodbye!");
    }

    static (Player player1, Player player2) SelectGameMode()
    {
        Console.WriteLine("\nSelect Game Mode:");
        Console.WriteLine("1. Human vs Human");
        Console.WriteLine("2. Human vs Bot");
        Console.WriteLine("3. Bot vs Bot");
        Console.WriteLine();

        int choice;
        while (true)
        {
            Console.Write("Enter your choice (1-3): ");
            string? input = Console.ReadLine();

            if (int.TryParse(input, out choice) && choice >= 1 && choice <= 3)
            {
                break;
            }

            Console.WriteLine("Invalid choice. Please enter 1, 2, or 3.");
        }

        Player player1;
        Player player2;

        switch (choice)
        {
            case 1: // Human vs Human
                Console.Write("\nEnter Player 1 name (Black): ");
                string name1 = Console.ReadLine() ?? "Player 1";
                if (string.IsNullOrWhiteSpace(name1)) name1 = "Player 1";

                Console.Write("Enter Player 2 name (White): ");
                string name2 = Console.ReadLine() ?? "Player 2";
                if (string.IsNullOrWhiteSpace(name2)) name2 = "Player 2";

                player1 = new HumanPlayer(name1, DiscColor.Black);
                player2 = new HumanPlayer(name2, DiscColor.White);
                break;

            case 2: // Human vs Bot
                Console.Write("\nEnter your name (Black): ");
                string humanName = Console.ReadLine() ?? "Player";
                if (string.IsNullOrWhiteSpace(humanName)) humanName = "Player";

                player1 = new HumanPlayer(humanName, DiscColor.Black);
                player2 = new BotPlayer("Bot", DiscColor.White);
                break;

            case 3: // Bot vs Bot
                player1 = new BotPlayer("Bot 1", DiscColor.Black);
                player2 = new BotPlayer("Bot 2", DiscColor.White);
                break;

            default:
                player1 = new HumanPlayer("Player 1", DiscColor.Black);
                player2 = new HumanPlayer("Player 2", DiscColor.White);
                break;
        }

        Console.WriteLine();
        return (player1, player2);
    }

    static bool AskPlayAgain()
    {
        Console.WriteLine();
        Console.Write("Do you want to play again? (y/n): ");
        string? input = Console.ReadLine();

        return input?.Trim().ToLower() == "y";
    }
}
