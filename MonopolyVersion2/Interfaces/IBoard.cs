using MonopolyApp.Interfaces;

namespace MonopolyApp.Enums
{
    public interface IBoard
    {
        int Width {get; set;}
        int Height {get; set;}
        ITile?[,] Grid {get; set;}
        List<ITile> Path {get; set;}
    }
}