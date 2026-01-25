using Othello.Enums;
using Othello.Models;

namespace Othello.Players;

public abstract class Player
{
    public string PlayerName { get; }
    public DiscColor DiscColor { get; }

    protected Player(string playerName, DiscColor discColor)
    {
        PlayerName = playerName;
        DiscColor = discColor;
    }

    public abstract (int row, int col) GetMove(Board board);
}
