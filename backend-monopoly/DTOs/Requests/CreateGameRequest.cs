namespace MonopolyBackend.DTOs.Requests
{
    public class CreateGameRequest
    {
        public List<string> PlayerNames { get; set; } = new();
    }
}
