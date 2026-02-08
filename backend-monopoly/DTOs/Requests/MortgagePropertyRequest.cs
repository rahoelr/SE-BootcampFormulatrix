namespace MonopolyBackend.DTOs.Requests
{
    public class MortgagePropertyRequest : PlayerActionRequest
    {
        public string PropertyName { get; set; } = string.Empty;
    }
}
