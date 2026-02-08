using MonopolyBackend.Services;

namespace MonopolyBackend.Services
{
    /// <summary>
    /// Manages single game instance (Option A - Hot-seat multiplayer)
    /// </summary>
    public class GameManager
    {
        private GameService? _currentGame;
        private readonly object _lock = new object();

        public GameService? GetGame()
        {
            lock (_lock)
            {
                return _currentGame;
            }
        }

        public GameService CreateGame(List<string> playerNames)
        {
            lock (_lock)
            {
                if (_currentGame != null)
                {
                    throw new InvalidOperationException("Game already exists. Reset first.");
                }

                // Validate player count
                if (playerNames.Count < 2 || playerNames.Count > 4)
                {
                    throw new ArgumentException("Player count must be between 2 and 4");
                }

                // Validate unique names
                if (playerNames.Distinct().Count() != playerNames.Count)
                {
                    throw new ArgumentException("Player names must be unique");
                }

                _currentGame = GameInitializationService.CreateGame(playerNames);
                return _currentGame;
            }
        }

        public void ResetGame()
        {
            lock (_lock)
            {
                _currentGame = null;
            }
        }

        public bool HasActiveGame()
        {
            lock (_lock)
            {
                return _currentGame != null;
            }
        }
    }
}
