using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace MiddlewareDemo;

public class FallbackExceptionHandler(
    IProblemDetailsService service,
    ILogger<FallbackExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception ex, CancellationToken ct)
    {
        logger.LogError(ex, "An unhandled exception occurred.");

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        await service.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Detail = "An unexpected error happened. Please try again."
            }
        });

        return true;
    }
}