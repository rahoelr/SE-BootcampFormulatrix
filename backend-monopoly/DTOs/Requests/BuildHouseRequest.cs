namespace MonopolyBackend.DTOs.Requests
{
    public class BuildHouseRequest : PlayerActionRequest
    {
        public string PropertyName { get; set; } = string.Empty;
    }
}
