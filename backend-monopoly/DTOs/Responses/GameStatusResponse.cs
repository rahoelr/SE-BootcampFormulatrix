namespace MonopolyBackend.DTOs.Responses
{
    public record GameStatusResponse
    {
        public bool HasActiveGame { get; init; }
    }
}
