using Othello.Interfaces;

namespace Othello.Models;

public class Cell : ICell
{
    public Position Position { get; }
    public Piece? Piece { get; set; }

    public Cell(Position position)
    {
        Position = position;
        Piece = null;
    }

    public bool IsEmpty => Piece == null;
}
