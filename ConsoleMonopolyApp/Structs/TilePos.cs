namespace ConsoleMonopolyApp.Structs;

public struct TilePos
{
    public int X { get; set; }
    public int Y { get; set; }

    public TilePos(int x, int y)
    {
        X = x;
        Y = y;
    }

    public override string ToString()
    {
        return $"({X}, {Y})";
    }

    public override bool Equals(object? obj)
    {
        if (obj is TilePos other)
        {
            return X == other.X && Y == other.Y;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }

    public static bool operator ==(TilePos left, TilePos right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(TilePos left, TilePos right)
    {
        return !(left == right);
    }
}
