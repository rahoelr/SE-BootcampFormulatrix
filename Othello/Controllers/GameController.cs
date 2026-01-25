using Othello.Enums;
using Othello.Models;
using Othello.Players;
using Othello.Views;

namespace Othello.Controllers;

public class GameController
{
    private readonly Board _board;
    private readonly Player _player1;
    private readonly Player _player2;
    private Player _currentPlayer;
    private readonly IGameView _view;
    private int _consecutiveSkips;

    public GameController(Player player1, Player player2, IGameView view)
    {
        _board = new Board();
        _player1 = player1;
        _player2 = player2;
        _currentPlayer = player1; // Black goes first
        _view = view;
        _consecutiveSkips = 0;
    }

    public void StartGame()
    {
        // Subscribe to events
        _board.OnBoardChanged += HandleBoardChanged;
        _board.OnDiscFlipped += HandleDiscFlipped;

        _view.ShowMessage("=== OTHELLO GAME ===");
        _view.ShowMessage($"Player 1: {_player1.PlayerName} (Black/B)");
        _view.ShowMessage($"Player 2: {_player2.PlayerName} (White/W)");
        _view.ShowMessage("Black moves first!\n");

        // Initial board render
        _board.InitBoard();

        // Game loop
        while (!IsGameOver())
        {
            PlayTurn();
        }

        // Show final result
        CheckWinner();

        // Unsubscribe from events
        _board.OnBoardChanged -= HandleBoardChanged;
        _board.OnDiscFlipped -= HandleDiscFlipped;
    }

    private void PlayTurn()
    {
        var validMoves = _board.GetValidMoves((char)_currentPlayer.DiscColor);

        if (validMoves.Count == 0)
        {
            _view.ShowMessage($"{_currentPlayer.PlayerName} ({(char)_currentPlayer.DiscColor}) has no valid moves. Turn skipped.");
            _consecutiveSkips++;
            SwitchTurn();
            return;
        }

        _consecutiveSkips = 0;

        _view.ShowMessage($"--- {_currentPlayer.PlayerName}'s turn ({(char)_currentPlayer.DiscColor}) ---");
        _view.ShowMessage($"Valid moves: {string.Join(", ", validMoves.Select(m => $"({m.row},{m.col})"))}");

        var (row, col) = _currentPlayer.GetMove(_board);

        int flipped = _board.ApplyMove(row, col, (char)_currentPlayer.DiscColor);
        _view.ShowMessage($"Placed disc at ({row}, {col}), flipped {flipped} disc(s).");

        SwitchTurn();
    }

    private void SwitchTurn()
    {
        _currentPlayer = _currentPlayer == _player1 ? _player2 : _player1;
    }

    private bool IsGameOver()
    {
        // Game over if both players skipped (no valid moves for either)
        if (_consecutiveSkips >= 2)
        {
            return true;
        }

        // Game over if board is full
        int blackCount = _board.CountDisc((char)DiscColor.Black);
        int whiteCount = _board.CountDisc((char)DiscColor.White);
        int emptyCount = _board.CountDisc((char)DiscColor.Empty);

        return emptyCount == 0;
    }

    private void CheckWinner()
    {
        int blackCount = _board.CountDisc((char)DiscColor.Black);
        int whiteCount = _board.CountDisc((char)DiscColor.White);

        _view.ShowResult(blackCount, whiteCount);

        if (blackCount > whiteCount)
        {
            _view.ShowMessage($"Congratulations {_player1.PlayerName}!");
        }
        else if (whiteCount > blackCount)
        {
            _view.ShowMessage($"Congratulations {_player2.PlayerName}!");
        }
    }

    private void HandleBoardChanged()
    {
        _view.RenderBoard(_board);
    }

    private void HandleDiscFlipped(int row, int col, char disc)
    {
        // This event can be used for animations or logging
        // For now, we just acknowledge the flip silently
        // The board change handler will re-render the board
    }
}
