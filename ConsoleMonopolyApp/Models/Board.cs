using ConsoleMonopolyApp.Interfaces;
using ConsoleMonopolyApp.Structs;

namespace ConsoleMonopolyApp.Models;

public class Board : IBoard
{
    public int Width { get; }
    public int Height { get; }
    public ITile?[,] Grid { get; }
    public List<ITile> Route { get; }

    public Board(int width, int height)
    {
        if (width < 3 || height < 3)
            throw new ArgumentException("Board must be at least 3x3");

        Width = width;
        Height = height;
        Grid = new ITile?[width, height];
        Route = new List<ITile>();
    }

    public void SetTile(int x, int y, ITile tile)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            throw new ArgumentOutOfRangeException($"Position ({x}, {y}) is out of bounds");

        tile.Pos = new TilePos(x, y);
        Grid[x, y] = tile;
    }

    public void AddToRoute(ITile tile)
    {
        tile.PathIndex = Route.Count;
        Route.Add(tile);
    }

    public ITile? GetTileAt(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            return null;

        return Grid[x, y];
    }

    public ITile? GetTileByPathIndex(int pathIndex)
    {
        if (pathIndex < 0)
            pathIndex = Route.Count + (pathIndex % Route.Count);
        
        pathIndex = pathIndex % Route.Count;
        
        if (pathIndex >= 0 && pathIndex < Route.Count)
            return Route[pathIndex];

        return null;
    }

    public int GetTotalPathLength()
    {
        return Route.Count;
    }
}
