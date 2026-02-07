namespace MonopolyBackend.Interfaces
{
    public interface IBoard
    {
        int Width {get; }
        int Height {get; }
        ITile?[,] Grid {get;}
        List<ITile> Path {get;}
    }
}