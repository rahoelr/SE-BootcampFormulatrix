using Othello.Enums;
using Othello.Models;

namespace Othello.Players;

public class HumanPlayer : Player
{
    public HumanPlayer(string playerName, DiscColor discColor) : base(playerName, discColor)
    {
    }

    public override (int row, int col) GetMove(Board board)
    {
        while (true)
        {
            Console.Write($"{PlayerName} ({(char)DiscColor}), enter your move (e.g., c3): ");
            string? input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("Invalid input. Please enter your move (e.g., 'c3').");
                continue;
            }

            input = input.Trim().ToLower();

            // Expected format: letter (a-h) followed by number (0-7)
            // e.g., "c3", "a0", "h7"
            if (input.Length < 2)
            {
                Console.WriteLine("Invalid input. Please enter column (a-h) and row (0-7) (e.g., 'c3').");
                continue;
            }

            char colChar = input[0];
            string rowStr = input.Substring(1);

            // Convert column letter to index (a=0, b=1, ..., h=7)
            if (colChar < 'a' || colChar > 'h')
            {
                Console.WriteLine("Invalid column. Please use letters a-h.");
                continue;
            }
            int col = colChar - 'a';

            // Parse row number
            if (!int.TryParse(rowStr, out int row))
            {
                Console.WriteLine("Invalid row. Please enter a number 0-7.");
                continue;
            }

            if (row < 0 || row >= board.Size || col < 0 || col >= board.Size)
            {
                Console.WriteLine($"Invalid position. Column must be a-h and row must be 0-7.");
                continue;
            }

            if (!board.IsValidMove(row, col, (char)DiscColor))
            {
                Console.WriteLine("Invalid move. You must place your disc to flip at least one opponent disc.");
                continue;
            }

            return (row, col);
        }
    }
}
