namespace MonopolyBackend.Structs
{
    public struct TilePos
{
    public int X;
    public int Y;

    public TilePos(int x, int y)
    {
        X = x;
        Y = y;
    }

    public override string ToString()
    {
        return $"({X}), {Y})";
    }
}
}