namespace MonopolyBackend.Models.Results
{
    public record PropertyActionResult
    {
        public bool Success { get; init; }
        public string Message { get; init; } = string.Empty;
    }
}
