using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace MiddlewareDemo;

public class AppExceptionHandler(
    IProblemDetailsService service,
    ILogger<AppExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception ex,
        CancellationToken ct)
    {
        if (ex is not AppException appException)
            return false;

        logger.LogWarning(ex, "An unhandled error occurred.");

        context.Response.StatusCode = appException.StatusCode;

        await service.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails = new ProblemDetails
            {
                Status = appException.StatusCode,
                Detail = appException.Message
            }
        });

        return true;
    }
}