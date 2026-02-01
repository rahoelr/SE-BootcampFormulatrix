using Othello.Enums;
using Othello.Interfaces;

namespace Othello.Models;

public class Piece : IPiece
{
    public PieceColor Color { get; private set; }

    public Piece(PieceColor color)
    {
        Color = color;
    }

    public void Flip()
    {
        Color = Color == PieceColor.Black ? PieceColor.White : PieceColor.Black;
    }
}
