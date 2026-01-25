using Othello.Models;

namespace Othello.Views;

public interface IGameView
{
    void RenderBoard(Board board);
    void ShowMessage(string message);
    void ShowResult(int blackCount, int whiteCount);
}
