namespace AnheloPets.API.Exceptions;

public class UnauthorizedException : ApiException
{
    public UnauthorizedException(string message, string? errorCode = "UNAUTHORIZED")
        : base(message, 401, errorCode)
    {
    }
}
