namespace MonopolyBackend.DTOs.Responses
{
    public record ResetResponse
    {
        public bool Success { get; init; }
        public string Message { get; init; } = string.Empty;
    }
}
