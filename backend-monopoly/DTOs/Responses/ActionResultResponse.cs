namespace MonopolyBackend.DTOs.Responses
{
    public class ActionResultResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }
    }
}
