namespace MonopolyBackend.Enums
{
    public static class GameActions
    {
        public const string RollDice = "roll-dice";
        public const string PayJailFee = "pay-jail-fee";
        public const string TryRollDoubles = "try-roll-doubles";
        public const string UseJailCard = "use-jail-card";
        public const string BuyProperty = "buy-property";
        public const string BuildHouse = "build-house";
        public const string SellHouse = "sell-house";
        public const string MortgageProperty = "mortgage-property";
        public const string UnmortgageProperty = "unmortgage-property";
        public const string Trade = "trade";
        public const string EndTurn = "end-turn";
    }
}