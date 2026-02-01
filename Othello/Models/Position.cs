namespace Othello.Models;

public class Position
{
    public int Row { get; }
    public int Col { get; }

    public Position(int row, int col)
    {
        Row = row;
        Col = col;
    }

    public override bool Equals(object? obj)
    {
        if (obj is Position other)
        {
            return Row == other.Row && Col == other.Col;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Row, Col);
    }

    public override string ToString()
    {
        return $"({Row}, {Col})";
    }
}
