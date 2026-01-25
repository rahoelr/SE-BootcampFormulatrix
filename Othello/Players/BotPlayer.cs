using Othello.Enums;
using Othello.Models;

namespace Othello.Players;

public class BotPlayer : Player
{
    private static readonly Random _random = new();

    // Corner positions are strategically valuable
    private static readonly (int row, int col)[] Corners = { (0, 0), (0, 7), (7, 0), (7, 7) };

    public BotPlayer(string playerName, DiscColor discColor) : base(playerName, discColor)
    {
    }

    public override (int row, int col) GetMove(Board board)
    {
        var validMoves = board.GetValidMoves((char)DiscColor);

        if (validMoves.Count == 0)
        {
            return (-1, -1); // No valid moves
        }

        // Strategy 1: Prioritize corners
        foreach (var corner in Corners)
        {
            if (validMoves.Contains(corner))
            {
                Console.WriteLine($"{PlayerName} ({(char)DiscColor}) plays: {corner.row} {corner.col} (corner)");
                return corner;
            }
        }

        // Strategy 2: Pick the move that flips the most discs
        var bestMoves = new List<(int row, int col)>();
        int maxFlips = 0;

        foreach (var move in validMoves)
        {
            int flips = board.GetPotentialFlips(move.row, move.col, (char)DiscColor);
            if (flips > maxFlips)
            {
                maxFlips = flips;
                bestMoves.Clear();
                bestMoves.Add(move);
            }
            else if (flips == maxFlips)
            {
                bestMoves.Add(move);
            }
        }

        // Strategy 3: Random selection among best moves
        var selectedMove = bestMoves[_random.Next(bestMoves.Count)];
        Console.WriteLine($"{PlayerName} ({(char)DiscColor}) plays: {selectedMove.row} {selectedMove.col} (flips {maxFlips})");

        return selectedMove;
    }
}
