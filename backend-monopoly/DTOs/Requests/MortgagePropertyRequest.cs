namespace MonopolyBackend.DTOs.Requests
{
    public record MortgagePropertyRequest
    {
        public string PropertyName { get; init; } = string.Empty;
        public string PlayerName { get; init; } = string.Empty;
    }
}
