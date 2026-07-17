using System.Text.Json;
using AnheloPets.API.DTOs;
using AnheloPets.API.Exceptions;

namespace AnheloPets.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        ErrorResponseDto response;

        if (exception is ApiException apiException)
        {
            context.Response.StatusCode = apiException.StatusCode;
            response = new ErrorResponseDto
            {
                StatusCode = apiException.StatusCode,
                Message = apiException.Message,
                ErrorCode = apiException.ErrorCode,
                Errors = apiException.Errors
            };

            LogException(apiException.StatusCode, apiException.Message, apiException.ErrorCode);
        }
        else
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            response = new ErrorResponseDto
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = "Ocurrió un error interno en el servidor",
                ErrorCode = "INTERNAL_SERVER_ERROR"
            };

            LogException(StatusCodes.Status500InternalServerError, exception.Message, "INTERNAL_SERVER_ERROR", exception);
        }

        if (_environment.IsDevelopment())
        {
            response.StackTrace = exception.StackTrace;
        }

        return context.Response.WriteAsJsonAsync(response);
    }

    private void LogException(int statusCode, string message, string? errorCode, Exception? exception = null)
    {
        string logMessage = $"[{statusCode}] {errorCode}: {message}";

        switch (statusCode)
        {
            case StatusCodes.Status400BadRequest:
                _logger.LogWarning(exception, logMessage);
                break;
            case StatusCodes.Status401Unauthorized:
                _logger.LogWarning(exception, logMessage);
                break;
            case StatusCodes.Status404NotFound:
                _logger.LogWarning(logMessage);
                break;
            default:
                _logger.LogError(exception, logMessage);
                break;
        }
    }
}
