namespace MonopolyBackend.DTOs.Responses
{
    public record ActionResultResponse
    {
        public bool Success { get; init; }
        public string Message { get; init; } = string.Empty;
        public object? Data { get; init; }
    }
}
