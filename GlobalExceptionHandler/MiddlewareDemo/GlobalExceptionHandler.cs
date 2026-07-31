using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace MiddlewareDemo;

public class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception ex,
        CancellationToken cancellationToken)
    {
        logger.LogError(ex, "An unhandled exception occurred.");

        var (statusCode, detail) = ex switch
        {
            ArgumentException => (StatusCodes.Status400BadRequest, ex.Message),
            NoEntityFoundException => (StatusCodes.Status404NotFound, ex.Message),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.")
        };

        context.Response.StatusCode = statusCode;

        await problemDetailsService.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails = new ProblemDetails
            {
                Status = statusCode,
                Detail = detail,
                Extensions =
                {
                    ["instance"] = context.Request.Path.Value
                }
            }
        });

        return true;
    }
}