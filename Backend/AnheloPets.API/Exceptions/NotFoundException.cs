namespace AnheloPets.API.Exceptions;

public class NotFoundException : ApiException
{
    public NotFoundException(string message, string? errorCode = "NOT_FOUND")
        : base(message, 404, errorCode)
    {
    }
}
