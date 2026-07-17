namespace AnheloPets.API.Exceptions;

public class BadRequestException : ApiException
{
    public BadRequestException(string message, string? errorCode = "BAD_REQUEST", Dictionary<string, string[]>? errors = null)
        : base(message, 400, errorCode, errors)
    {
    }
}
