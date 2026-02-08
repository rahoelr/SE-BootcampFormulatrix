namespace MonopolyBackend.DTOs.Requests
{
    public record BuildHouseRequest : PlayerActionRequest
    {
        public string PropertyName { get; init; } = string.Empty;
    }
}
