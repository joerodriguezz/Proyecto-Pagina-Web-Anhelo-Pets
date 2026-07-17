namespace AnheloPets.API.Exceptions;

public class ApiException : Exception
{
    public int StatusCode { get; set; }
    public string? ErrorCode { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }

    public ApiException(string message, int statusCode = 500, string? errorCode = null, Dictionary<string, string[]>? errors = null)
        : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
        Errors = errors;
    }

    public ApiException(string message, Exception innerException, int statusCode = 500, string? errorCode = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }
}
