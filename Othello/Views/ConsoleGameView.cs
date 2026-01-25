using Othello.Enums;
using Othello.Models;

namespace Othello.Views;

public class ConsoleGameView : IGameView
{
    public void RenderBoard(Board board)
    {
        Console.WriteLine();
        Console.WriteLine("  a b c d e f g h");
        Console.WriteLine("  ---------------");

        for (int row = 0; row < board.Size; row++)
        {
            Console.Write($"{row}|");
            for (int col = 0; col < board.Size; col++)
            {
                char cell = board.GetCell(row, col);

                // Set color for visual clarity
                if (cell == (char)DiscColor.Black)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                }
                else if (cell == (char)DiscColor.White)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                }

                Console.Write($"{cell} ");
                Console.ResetColor();
            }
            Console.WriteLine($"|{row}");
        }

        Console.WriteLine("  ---------------");
        Console.WriteLine("  a b c d e f g h");
        Console.WriteLine();

        // Show current disc count
        int blackCount = board.CountDisc((char)DiscColor.Black);
        int whiteCount = board.CountDisc((char)DiscColor.White);
        Console.WriteLine($"Score - Black (B): {blackCount} | White (W): {whiteCount}");
        Console.WriteLine();
    }

    public void ShowMessage(string message)
    {
        Console.WriteLine(message);
    }

    public void ShowResult(int blackCount, int whiteCount)
    {
        Console.WriteLine();
        Console.WriteLine("========== GAME OVER ==========");
        Console.WriteLine($"Final Score - Black (B): {blackCount} | White (W): {whiteCount}");
        Console.WriteLine();

        if (blackCount > whiteCount)
        {
            Console.WriteLine("*** BLACK WINS! ***");
        }
        else if (whiteCount > blackCount)
        {
            Console.WriteLine("*** WHITE WINS! ***");
        }
        else
        {
            Console.WriteLine("*** IT'S A TIE! ***");
        }

        Console.WriteLine("================================");
    }
}
