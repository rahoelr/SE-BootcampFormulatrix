using MonopolyApp.Interfaces;

namespace MonopolyApp.Interfaces
{
    public interface IView
    {
        // Display methods
        void ClearScreen();
        void DrawBoard(IBoard board, List<IPlayer> players);
        void ShowPlayerInfo(IPlayer player, int playerMoney);
        void ShowAllPlayersInfo(List<IPlayer> players, Dictionary<IPlayer, int> playerMoney);
        void ShowMessage(string message);
        void ShowError(string message);
        void ShowWarning(string message);
        void ShowSuccess(string message);
        void ShowDiceRoll(int dice1, int dice2);
        void ShowCard(ICard card);
        void ShowPropertyDetails(IAsset asset);
        void ShowTradeOffer(IPlayer from, IPlayer to, List<IAsset> offerFrom, int moneyFrom, List<IAsset> offerTo, int moneyTo);
        void ShowGameOver(IPlayer winner, int winnerMoney);
        void ShowWelcome();
        void ShowTurnHeader(string playerName);

        void WaitForKeyPress();
        void ShowMenu(string title, List<string> options);
        int GetPlayerChoice(int maxOptions);
        string GetPlayerInput(string prompt);
        bool GetYesNo(string prompt);

        // Player setup methods
        int GetPlayerCount(int min, int max);
        string GetPlayerName(int playerIndex);

        // Property selection methods
        int? SelectFromPropertyList(List<IAsset> assets, string title, Func<IAsset, string> formatter);
        List<IAsset> SelectMultipleFromPropertyList(List<IAsset> assets, string prompt, Func<IAsset, string> formatter);
        
        // Player selection
        IPlayer? SelectPlayer(List<IPlayer> players, string prompt, Func<IPlayer, string> formatter);
        
        // Money input
        int GetMoneyAmount(string prompt);
    }
}
