using Othello.Enums;

namespace Othello.Interfaces;

public interface IPlayer
{
    string Name { get; }
    PlayerColor Color { get; }
}
