using Othello.Enums;
using Othello.Interfaces;
using Othello.Models;

namespace Othello.Controllers;

public class GameController
{
    private readonly Board _board;
    private readonly List<IPlayer> _players;
    private int _currentPlayerIndex;
    private bool _isGameOver;

    public event Action<IPlayer>? TurnChanged;
    public event Action<IBoard>? BoardUpdated;
    public event Action<IPlayer?>? GameEnded;

    public bool IsGameOver => _isGameOver;
    public IPlayer CurrentPlayer => _players[_currentPlayerIndex];
    public IBoard Board => _board;
    public IReadOnlyList<IPlayer> Players => _players;

    private static readonly Dictionary<Direction, (int rowDelta, int colDelta)> DirectionOffsets = new()
    {
        { Direction.North, (-1, 0) },
        { Direction.South, (1, 0) },
        { Direction.East, (0, 1) },
        { Direction.West, (0, -1) },
        { Direction.NorthEast, (-1, 1) },
        { Direction.NorthWest, (-1, -1) },
        { Direction.SouthEast, (1, 1) },
        { Direction.SouthWest, (1, -1) }
    };

    public GameController(List<IPlayer> players, Board board)
    {
        _players = players;
        _board = board;
        _currentPlayerIndex = 0;
        _isGameOver = false;
    }

    public void StartGame()
    {
        InitializeBoard();
        _currentPlayerIndex = 0; // Black goes first
        _isGameOver = false;
        RaiseBoardUpdated();
        RaiseTurnChanged();
    }

    private void InitializeBoard()
    {
        int mid = _board.Size / 2;
        
        // Set up the initial 4 pieces in the center
        // Standard Othello starting position
        _board.GetCell(mid - 1, mid - 1).Piece = new Piece(PieceColor.White);
        _board.GetCell(mid - 1, mid).Piece = new Piece(PieceColor.Black);
        _board.GetCell(mid, mid - 1).Piece = new Piece(PieceColor.Black);
        _board.GetCell(mid, mid).Piece = new Piece(PieceColor.White);
    }

    public bool PlayAt(Position position)
    {
        if (_isGameOver)
            return false;

        var playerColor = CurrentPlayer.Color;
        
        if (!IsValidMove(position, playerColor))
            return false;

        var flippablePositions = GetFlippablePositions(position, playerColor);
        
        if (flippablePositions.Count == 0)
            return false;

        PlacePiece(position, playerColor);
        FlipPieces(flippablePositions);
        
        RaiseBoardUpdated();
        SwitchTurn();
        
        return true;
    }

    public void PassTurn()
    {
        if (_isGameOver)
            return;

        // Only allow pass if there are no valid moves
        if (GetValidMoves(CurrentPlayer.Color).Count > 0)
            return;

        SwitchTurn();
    }

    public List<Position> GetValidMoves(PlayerColor color)
    {
        var validMoves = new List<Position>();
        
        for (int row = 0; row < _board.Size; row++)
        {
            for (int col = 0; col < _board.Size; col++)
            {
                var position = new Position(row, col);
                if (IsValidMove(position, color))
                {
                    validMoves.Add(position);
                }
            }
        }
        
        return validMoves;
    }

    private bool IsValidMove(Position pos, PlayerColor color)
    {
        // Check if position is within bounds
        if (!_board.IsValidPosition(pos))
            return false;

        // Check if cell is empty
        var cell = _board.GetCell(pos);
        if (!cell.IsEmpty)
            return false;

        // Check if this move would flip at least one piece
        return GetFlippablePositions(pos, color).Count > 0;
    }

    private List<Position> GetFlippablePositions(Position pos, PlayerColor color)
    {
        var allFlippable = new List<Position>();
        var pieceColor = PlayerColorToPieceColor(color);
        var opponentColor = GetOpponentPieceColor(color);

        foreach (var direction in DirectionOffsets.Values)
        {
            var flippableInDirection = GetFlippableInDirection(pos, direction, pieceColor, opponentColor);
            allFlippable.AddRange(flippableInDirection);
        }

        return allFlippable;
    }

    private List<Position> GetFlippableInDirection(Position start, (int rowDelta, int colDelta) direction, 
        PieceColor playerColor, PieceColor opponentColor)
    {
        var flippable = new List<Position>();
        int row = start.Row + direction.rowDelta;
        int col = start.Col + direction.colDelta;

        // Collect opponent pieces in this direction
        while (_board.IsValidPosition(row, col))
        {
            var cell = _board.GetCell(row, col);
            
            if (cell.IsEmpty)
                return new List<Position>(); // Empty cell, no capture possible
            
            if (cell.Piece!.Color == opponentColor)
            {
                flippable.Add(new Position(row, col));
            }
            else if (cell.Piece.Color == playerColor)
            {
                // Found our own piece, return collected opponent pieces
                return flippable;
            }
            
            row += direction.rowDelta;
            col += direction.colDelta;
        }

        // Reached edge without finding our piece
        return new List<Position>();
    }

    private void PlacePiece(Position pos, PlayerColor color)
    {
        var pieceColor = PlayerColorToPieceColor(color);
        _board.GetCell(pos).Piece = new Piece(pieceColor);
    }

    private void FlipPieces(List<Position> positions)
    {
        foreach (var pos in positions)
        {
            var cell = _board.GetCell(pos);
            cell.Piece?.Flip();
        }
    }

    public int GetScore(IPlayer player)
    {
        return CountPieces(PlayerColorToPieceColor(player.Color));
    }

    private int CountPieces(PieceColor color)
    {
        int count = 0;
        for (int row = 0; row < _board.Size; row++)
        {
            for (int col = 0; col < _board.Size; col++)
            {
                var cell = _board.GetCell(row, col);
                if (!cell.IsEmpty && cell.Piece!.Color == color)
                {
                    count++;
                }
            }
        }
        return count;
    }

    private void SwitchTurn()
    {
        // Switch to next player
        _currentPlayerIndex = (_currentPlayerIndex + 1) % _players.Count;
        
        // Check if current player has valid moves
        if (GetValidMoves(CurrentPlayer.Color).Count == 0)
        {
            // Switch back to previous player
            _currentPlayerIndex = (_currentPlayerIndex + 1) % _players.Count;
            
            // Check if previous player also has no valid moves
            if (GetValidMoves(CurrentPlayer.Color).Count == 0)
            {
                // Neither player can move, game over
                _isGameOver = true;
                RaiseGameEnded(GetWinner());
                return;
            }
        }
        
        RaiseTurnChanged();
    }

    private bool CheckGameOver()
    {
        // Game is over if neither player can make a valid move
        foreach (var player in _players)
        {
            if (GetValidMoves(player.Color).Count > 0)
                return false;
        }
        return true;
    }

    public IPlayer? GetWinner()
    {
        int blackScore = CountPieces(PieceColor.Black);
        int whiteScore = CountPieces(PieceColor.White);

        if (blackScore > whiteScore)
            return _players.FirstOrDefault(p => p.Color == PlayerColor.Black);
        else if (whiteScore > blackScore)
            return _players.FirstOrDefault(p => p.Color == PlayerColor.White);
        
        return null; // Draw
    }

    private static PieceColor PlayerColorToPieceColor(PlayerColor color)
    {
        return color == PlayerColor.Black ? PieceColor.Black : PieceColor.White;
    }

    private static PieceColor GetOpponentPieceColor(PlayerColor color)
    {
        return color == PlayerColor.Black ? PieceColor.White : PieceColor.Black;
    }

    private void RaiseTurnChanged()
    {
        TurnChanged?.Invoke(CurrentPlayer);
    }

    private void RaiseBoardUpdated()
    {
        BoardUpdated?.Invoke(_board);
    }

    private void RaiseGameEnded(IPlayer? winner)
    {
        GameEnded?.Invoke(winner);
    }
}
