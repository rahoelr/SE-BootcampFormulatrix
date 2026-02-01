using Othello.Models;

namespace Othello.Interfaces;

public interface IBoard
{
    int Size { get; }
    Cell[,] Cells { get; }
}
