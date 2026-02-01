using Othello.Controllers;
using Othello.Enums;
using Othello.Interfaces;
using Othello.Models;

namespace Othello.Views;

public class ConsoleView
{
    private readonly GameController _gameController;
    
    private const string BlackPiece = "●";
    private const string WhitePiece = "○";
    private const string EmptyCell = ".";
    private const string ValidMove = "*";

    public ConsoleView(GameController gameController)
    {
        _gameController = gameController;
        
        // Subscribe to events
        _gameController.TurnChanged += OnTurnChanged;
        _gameController.BoardUpdated += OnBoardUpdated;
        _gameController.GameEnded += OnGameEnded;
    }

    public void Run()
    {
        Console.Clear();
        DisplayWelcome();
        _gameController.StartGame();
        
        while (!_gameController.IsGameOver)
        {
            DisplayBoard();
            DisplayScore();
            DisplayCurrentPlayer();
            DisplayValidMoves();
            
            var input = GetPlayerInput();
            ProcessInput(input);
        }
        
        // Final display after game ends
        DisplayBoard();
        DisplayScore();
        DisplayGameOver();
    }

    private void DisplayWelcome()
    {
        Console.WriteLine("================================");
        Console.WriteLine("     WELCOME TO OTHELLO");
        Console.WriteLine("================================");
        Console.WriteLine();
        Console.WriteLine($"Black: {BlackPiece}  |  White: {WhitePiece}");
        Console.WriteLine($"Valid moves shown as: {ValidMove}");
        Console.WriteLine();
        Console.WriteLine("Enter moves like 'D3', 'A1', etc.");
        Console.WriteLine("Type 'quit' to exit the game.");
        Console.WriteLine();
        Console.WriteLine("Press any key to start...");
        Console.ReadKey(true);
    }

    private void DisplayBoard()
    {
        Console.Clear();
        var board = (Board)_gameController.Board;
        var validMoves = _gameController.GetValidMoves(_gameController.CurrentPlayer.Color);
        
        Console.WriteLine();
        Console.WriteLine("  A B C D E F G H");
        
        for (int row = 0; row < board.Size; row++)
        {
            Console.Write($"{row + 1} ");
            
            for (int col = 0; col < board.Size; col++)
            {
                var cell = board.GetCell(row, col);
                var position = new Position(row, col);
                
                if (cell.IsEmpty)
                {
                    // Check if this is a valid move
                    bool isValidMove = validMoves.Any(p => p.Row == row && p.Col == col);
                    Console.Write(isValidMove ? ValidMove : EmptyCell);
                }
                else
                {
                    Console.Write(cell.Piece!.Color == PieceColor.Black ? BlackPiece : WhitePiece);
                }
                
                Console.Write(" ");
            }
            
            Console.WriteLine();
        }
        
        Console.WriteLine();
    }

    private void DisplayScore()
    {
        var blackPlayer = GetPlayerByColor(PlayerColor.Black);
        var whitePlayer = GetPlayerByColor(PlayerColor.White);
        
        int blackScore = _gameController.GetScore(blackPlayer!);
        int whiteScore = _gameController.GetScore(whitePlayer!);
        
        Console.WriteLine($"Score: {blackPlayer!.Name}({BlackPiece}): {blackScore} | {whitePlayer!.Name}({WhitePiece}): {whiteScore}");
    }

    private void DisplayCurrentPlayer()
    {
        var current = _gameController.CurrentPlayer;
        string symbol = current.Color == PlayerColor.Black ? BlackPiece : WhitePiece;
        Console.WriteLine($"Current Turn: {current.Name} ({symbol})");
    }

    private void DisplayValidMoves()
    {
        var validMoves = _gameController.GetValidMoves(_gameController.CurrentPlayer.Color);
        
        if (validMoves.Count == 0)
        {
            Console.WriteLine("No valid moves available. Type 'pass' to skip turn.");
        }
        else
        {
            Console.Write("Valid moves: ");
            var moveStrings = validMoves.Select(p => PositionToNotation(p));
            Console.WriteLine(string.Join(", ", moveStrings));
        }
    }

    private string GetPlayerInput()
    {
        Console.WriteLine();
        Console.Write("Enter move (e.g. D3) or 'pass': ");
        return Console.ReadLine()?.Trim().ToUpper() ?? "";
    }

    private void ProcessInput(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            DisplayMessage("Invalid input. Please try again.");
            return;
        }

        if (input == "QUIT" || input == "EXIT")
        {
            Console.WriteLine("Thanks for playing!");
            Environment.Exit(0);
        }

        if (input == "PASS")
        {
            var validMoves = _gameController.GetValidMoves(_gameController.CurrentPlayer.Color);
            if (validMoves.Count > 0)
            {
                DisplayMessage("You have valid moves available. You cannot pass.");
                return;
            }
            _gameController.PassTurn();
            return;
        }

        var position = ParseInput(input);
        if (position == null)
        {
            DisplayMessage("Invalid format. Use format like 'D3' or 'A1'.");
            return;
        }

        if (!_gameController.PlayAt(position))
        {
            DisplayMessage("Invalid move. Please choose a valid position.");
        }
    }

    private Position? ParseInput(string input)
    {
        if (input.Length < 2 || input.Length > 2)
            return null;

        char colChar = input[0];
        char rowChar = input[1];

        // Column: A-H -> 0-7
        if (colChar < 'A' || colChar > 'H')
            return null;

        // Row: 1-8 -> 0-7
        if (rowChar < '1' || rowChar > '8')
            return null;

        int col = colChar - 'A';
        int row = rowChar - '1';

        return new Position(row, col);
    }

    private string PositionToNotation(Position pos)
    {
        char col = (char)('A' + pos.Col);
        int row = pos.Row + 1;
        return $"{col}{row}";
    }

    private void DisplayMessage(string message)
    {
        Console.WriteLine();
        Console.WriteLine(message);
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey(true);
    }

    private void DisplayGameOver()
    {
        Console.WriteLine();
        Console.WriteLine("================================");
        Console.WriteLine("         GAME OVER!");
        Console.WriteLine("================================");
        
        var winner = _gameController.GetWinner();
        if (winner != null)
        {
            string symbol = winner.Color == PlayerColor.Black ? BlackPiece : WhitePiece;
            Console.WriteLine($"Winner: {winner.Name} ({symbol})");
        }
        else
        {
            Console.WriteLine("It's a DRAW!");
        }
        
        Console.WriteLine();
        Console.WriteLine("Thanks for playing!");
    }

    private IPlayer? GetPlayerByColor(PlayerColor color)
    {
        return _gameController.Players.FirstOrDefault(p => p.Color == color);
    }

    // Event handlers
    private void OnTurnChanged(IPlayer player)
    {
        // Can be used for additional notifications
    }

    private void OnBoardUpdated(IBoard board)
    {
        // Can be used for additional updates
    }

    private void OnGameEnded(IPlayer? winner)
    {
        // Handled in main loop
    }
}
