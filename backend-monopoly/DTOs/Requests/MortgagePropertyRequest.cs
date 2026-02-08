namespace MonopolyBackend.DTOs.Requests
{
    public record MortgagePropertyRequest : PlayerActionRequest
    {
        public string PropertyName { get; init; } = string.Empty;
    }
}
