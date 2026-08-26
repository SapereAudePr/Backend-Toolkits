using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Web.Middleware;

public class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IHostEnvironment env) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception ex,
        CancellationToken ct)
    {
        logger.LogError(ex, "An unhandled exception occurred: {ExMessage}", ex.Message);

        var (statusCode, title) = MapException(ex);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = env.IsDevelopment() ? ex.ToString() : "An unexpected error occurred",
            Instance = context.Request.Path
        };

        context.Response.StatusCode = statusCode;

        await context.Response.WriteAsJsonAsync(problemDetails, ct);

        return true;
    }

    private static (int StatusCode, string Title) MapException(Exception ex) =>
        ex switch
        {
            ArgumentException =>
                (StatusCodes.Status400BadRequest, "Invalid argument"),
            UnauthorizedAccessException =>
                (StatusCodes.Status401Unauthorized, "Unauthorized"),
            KeyNotFoundException =>
                (StatusCodes.Status404NotFound, "Resource not found"),
            InvalidOperationException =>
                (StatusCodes.Status409Conflict, "Invalid operation"),
            TimeoutException =>
                (StatusCodes.Status504GatewayTimeout, "Request timed out"),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error happened")
        };
}