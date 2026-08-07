using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace MiddlewareDemo;

/// <summary>
/// The first handler in the chain responsible
/// for <seealso cref="AppException"/> exceptions
/// </summary>
/// <param name="service">To create a JSON response to the client</param>
/// <param name="logger">For debugging purpose</param>
public class AppExceptionHandler(
    IProblemDetailsService service,
    ILogger<AppExceptionHandler> logger) : IExceptionHandler
{
    /// <summary>
    /// Checks whether the exception is an <seealso cref="AppException"/>.
    /// If it is, logs it and returns a <seealso cref="ProblemDetails"/> response.
    /// Otherwise, returns false so fallback handler can handle it.
    /// </summary>
    /// <param name="context">The current HTTP context for the incoming request.</param>
    /// <param name="ex">The exception that was thrown while processing the request.</param>
    /// <param name="ct">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    /// A <see cref="ValueTask{Boolean}"/> that returns:
    /// <list type="bullet">
    /// <item>
    /// <description><c>true</c> if the exception is an <see cref="AppException"/> and was handled.</description>
    /// </item>
    /// <item>
    /// <description><c>false</c> if the exception is not an <see cref="AppException"/>, allowing the next exception handler to process it.</description>
    /// </item>
    /// </list>
    /// </returns>
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception ex,
        CancellationToken ct)
    {
        if (ex is not AppException appException)
            return false;

        logger.LogWarning(ex, "An unhandled error for trace {TraceId}",
            Activity.Current?.Id ?? context.TraceIdentifier);

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