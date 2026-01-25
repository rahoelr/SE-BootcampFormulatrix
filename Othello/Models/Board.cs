using Othello.Enums;

namespace Othello.Models;

public class Board
{
    private const int BoardSize = 8;
    private readonly char[,] _board;

    // Direction vectors for 8 directions: N, NE, E, SE, S, SW, W, NW
    private static readonly int[] DirRow = { -1, -1, 0, 1, 1, 1, 0, -1 };
    private static readonly int[] DirCol = { 0, 1, 1, 1, 0, -1, -1, -1 };

    public event Action? OnBoardChanged;
    public event Action<int, int, char>? OnDiscFlipped;

    public int Size => BoardSize;

    public Board()
    {
        _board = new char[BoardSize, BoardSize];
        InitBoard();
    }

    public void InitBoard()
    {
        // Fill all cells with empty
        for (int row = 0; row < BoardSize; row++)
        {
            for (int col = 0; col < BoardSize; col++)
            {
                _board[row, col] = (char)DiscColor.Empty;
            }
        }

        // Set up initial 4 discs in the center
        _board[3, 3] = (char)DiscColor.White;
        _board[3, 4] = (char)DiscColor.Black;
        _board[4, 3] = (char)DiscColor.Black;
        _board[4, 4] = (char)DiscColor.White;

        OnBoardChanged?.Invoke();
    }

    public char GetCell(int row, int col)
    {
        if (row < 0 || row >= BoardSize || col < 0 || col >= BoardSize)
        {
            return '\0';
        }
        return _board[row, col];
    }

    public bool IsValidMove(int row, int col, char disc)
    {
        // Check bounds
        if (row < 0 || row >= BoardSize || col < 0 || col >= BoardSize)
        {
            return false;
        }

        // Cell must be empty
        if (_board[row, col] != (char)DiscColor.Empty)
        {
            return false;
        }

        // Check if move would flip at least one disc in any direction
        char opponent = GetOpponentDisc(disc);

        for (int dir = 0; dir < 8; dir++)
        {
            if (CountFlipsInDirection(row, col, DirRow[dir], DirCol[dir], disc, opponent) > 0)
            {
                return true;
            }
        }

        return false;
    }

    private int CountFlipsInDirection(int row, int col, int dRow, int dCol, char disc, char opponent)
    {
        int r = row + dRow;
        int c = col + dCol;
        int count = 0;

        // Count opponent discs in this direction
        while (r >= 0 && r < BoardSize && c >= 0 && c < BoardSize && _board[r, c] == opponent)
        {
            count++;
            r += dRow;
            c += dCol;
        }

        // Check if the line ends with our disc
        if (count > 0 && r >= 0 && r < BoardSize && c >= 0 && c < BoardSize && _board[r, c] == disc)
        {
            return count;
        }

        return 0;
    }

    public int ApplyMove(int row, int col, char disc)
    {
        if (!IsValidMove(row, col, disc))
        {
            return 0;
        }

        char opponent = GetOpponentDisc(disc);
        int totalFlipped = 0;

        // Place the disc
        _board[row, col] = disc;

        // Flip discs in all valid directions
        for (int dir = 0; dir < 8; dir++)
        {
            int flips = CountFlipsInDirection(row, col, DirRow[dir], DirCol[dir], disc, opponent);
            if (flips > 0)
            {
                // Flip the discs
                int r = row + DirRow[dir];
                int c = col + DirCol[dir];
                for (int i = 0; i < flips; i++)
                {
                    _board[r, c] = disc;
                    OnDiscFlipped?.Invoke(r, c, disc);
                    r += DirRow[dir];
                    c += DirCol[dir];
                    totalFlipped++;
                }
            }
        }

        OnBoardChanged?.Invoke();
        return totalFlipped;
    }

    public int CountDisc(char disc)
    {
        int count = 0;
        for (int row = 0; row < BoardSize; row++)
        {
            for (int col = 0; col < BoardSize; col++)
            {
                if (_board[row, col] == disc)
                {
                    count++;
                }
            }
        }
        return count;
    }

    public List<(int row, int col)> GetValidMoves(char disc)
    {
        var validMoves = new List<(int row, int col)>();

        for (int row = 0; row < BoardSize; row++)
        {
            for (int col = 0; col < BoardSize; col++)
            {
                if (IsValidMove(row, col, disc))
                {
                    validMoves.Add((row, col));
                }
            }
        }

        return validMoves;
    }

    public int GetPotentialFlips(int row, int col, char disc)
    {
        if (!IsValidMove(row, col, disc))
        {
            return 0;
        }

        char opponent = GetOpponentDisc(disc);
        int totalFlips = 0;

        for (int dir = 0; dir < 8; dir++)
        {
            totalFlips += CountFlipsInDirection(row, col, DirRow[dir], DirCol[dir], disc, opponent);
        }

        return totalFlips;
    }

    private static char GetOpponentDisc(char disc)
    {
        return disc == (char)DiscColor.Black ? (char)DiscColor.White : (char)DiscColor.Black;
    }
}
