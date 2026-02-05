using ConsoleMonopolyApp.Views;
using MonopolyApp.Controllers;
using MonopolyApp.Models;
using MonopolyApp.Interfaces;
using MonopolyApp.Data;

namespace ConsoleMonopolyApp;

public class Program
{
    public static void Main(string[] args)
    {
        IView view = new ConsoleView();
        view.ShowWelcome();

        int numPlayers = view.GetPlayerCount(2, 4);
        if (numPlayers < 2)
        {
            view.ShowError("Perlu minimal 2 pemain untuk bermain!");
            return;
        }

        var players = new List<IPlayer>();
        for (int i = 0; i < numPlayers; i++)
        {
            string name = view.GetPlayerName(i + 1);
            players.Add(new Player(name, new Money(1500)));
        }

        //Setup game components
        var board = SetupBoard.CreateStandardBoard();
        var dices = new List<IDice> { new Dice(6), new Dice(6) };
        var communityChestDeck = SetupBoard.CreateCommunityChestDeck();
        var chanceDeck = SetupBoard.CreateChanceDeck();

        var game = new GameController(board, players, dices, communityChestDeck, chanceDeck, view);
        game.StartGame();

        while (!game.IsGameOver)
        {
            game.PlayTurn();
        }

        // Show final game over screen
        if (game.Winner != null)
        {
            view.ShowGameOver(game.Winner, game.GetPlayerMoney(game.Winner));
        }

        view.ShowMessage("\nTerima kasih sudah bermain Monopoly!");
        view.ShowMessage("Tekan tombol apa saja untuk keluar...");
        view.WaitForKeyPress();
    }
}
