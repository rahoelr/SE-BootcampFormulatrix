namespace ConsoleMonopolyApp.Interfaces;

public interface IBoard
{
    int Width { get; }
    int Height { get; }
    ITile?[,] Grid { get; }
    List<ITile> Route { get; }
    
    ITile? GetTileAt(int x, int y);
    ITile? GetTileByPathIndex(int pathIndex);
}
