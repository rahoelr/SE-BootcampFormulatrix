using Othello.Models;

namespace Othello.Interfaces;

public interface ICell
{
    Position Position { get; }
    Piece? Piece { get; set; }
}
