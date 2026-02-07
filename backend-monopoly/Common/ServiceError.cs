namespace MonopolyBackend.Common
{
    public record ServiceError(ErrorType Type, string Message);

    public enum ErrorType
    {
        Validation,
        NotFound,
        Conflict,
        Unauthorized,
        Unexpected
    }

}