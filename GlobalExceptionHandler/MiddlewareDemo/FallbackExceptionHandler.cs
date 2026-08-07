using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace MiddlewareDemo;

/// <summary>
/// Second handler in the chain handles any exception
/// that was not handled by <seealso cref="AppExceptionHandler"/>
/// </summary>
/// <param name="service">To create a JSON response to the client</param>
/// <param name="logger">For debugging purpose</param>
public class FallbackExceptionHandler(
    IProblemDetailsService service,
    ILogger<FallbackExceptionHandler> logger) : IExceptionHandler
{
    /// <summary>
    /// Handles all exceptions that are not handled by <seealso cref="AppExceptionHandler"/>,
    /// returning a generic <see cref="ProblemDetails"/> response with an
    /// HTTP 500 (Internal Server Error) status code.
    /// </summary>
    /// <remarks>
    /// This handler acts as the final fallback in the exception handling pipeline.
    /// It prevents unhandled exceptions from being exposed to the client while
    /// logging the error for diagnostics.
    /// </remarks>
    /// <param name="context">The current HTTP context for the incoming request.</param>
    /// <param name="ex">The exception that was thrown while processing the request.</param>
    /// <param name="ct">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    /// A <see cref="ValueTask{Boolean}"/> that always returns <c>true</c>,
    /// indicating that the exception has been handled.
    /// </returns>
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception ex, CancellationToken ct)
    {
        logger.LogError(ex, "An unhandled error for trace {TraceId}",
            Activity.Current?.Id ?? context.TraceIdentifier);

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