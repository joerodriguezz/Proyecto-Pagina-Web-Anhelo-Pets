namespace AnheloPets.API.DTOs;

public class ErrorResponseDto
{
    public DateTime Timestamp { get; set; }
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }
    public string? StackTrace { get; set; }

    public ErrorResponseDto()
    {
        Timestamp = DateTime.UtcNow;
    }
}
