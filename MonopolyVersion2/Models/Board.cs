using MonopolyApp.Enums;
using MonopolyApp.Interfaces;

namespace MonopolyApp.Models
{
    public class Board : IBoard
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public ITile?[,] Grid { get; set; }
        public List<ITile> Path { get; set; }

        public Board(int width, int height)
        {
            Width = width;
            Height = height;
            Grid = new ITile[height, width];
            Path = new List<ITile>();
        }
    }
}