using Serilog;

namespace MonopolyBackend.Services
{
    public class GameServiceManager
    {
        private GameService? _currentGame;

        public GameService? CurrentGame
        {
            get => _currentGame;
            set => _currentGame = value;
        }

        public bool HasActiveGame => _currentGame != null;

        public void Reset()
        {
            _currentGame = null;
        }
    }
}