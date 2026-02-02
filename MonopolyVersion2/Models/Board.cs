using MonopolyApp.Enums;
using MonopolyApp.Interfaces;

namespace MonopolyApp.Models
{
    public class Board : IBoard
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public ITile[,] Grid { get; set; }
        public List<ITile> Route { get; set; }

        public Board(int width, int height, ITile[,] grid, List<ITile> route)
        {
            Width = width;
            Height = height;
            Grid = grid;
            Route = route;
        }
    }
}