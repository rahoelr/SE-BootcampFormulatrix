namespace MonopolyBackend.DTOs.Requests
{
    public record CreateGameRequest
    {
        public List<string> PlayerNames { get; init; } = new();
    }
}
