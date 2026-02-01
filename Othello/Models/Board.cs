using Othello.Interfaces;

namespace Othello.Models;

public class Board : IBoard
{
    public int Size { get; }
    public Cell[,] Cells { get; }

    public Board(int size = 8)
    {
        Size = size;
        Cells = new Cell[size, size];
        InitializeCells();
    }

    private void InitializeCells()
    {
        for (int row = 0; row < Size; row++)
        {
            for (int col = 0; col < Size; col++)
            {
                Cells[row, col] = new Cell(new Position(row, col));
            }
        }
    }

    public Cell GetCell(Position position)
    {
        return Cells[position.Row, position.Col];
    }

    public Cell GetCell(int row, int col)
    {
        return Cells[row, col];
    }

    public bool IsValidPosition(Position position)
    {
        return position.Row >= 0 && position.Row < Size &&
               position.Col >= 0 && position.Col < Size;
    }

    public bool IsValidPosition(int row, int col)
    {
        return row >= 0 && row < Size && col >= 0 && col < Size;
    }
}
