namespace MonopolyBackend.DTOs.Requests
{
    public record PlayerActionRequest
    {
        public string PlayerName { get; init; } = string.Empty;
    }
}
