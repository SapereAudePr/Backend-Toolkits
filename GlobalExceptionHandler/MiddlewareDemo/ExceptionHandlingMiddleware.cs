using Microsoft.AspNetCore.Mvc;

namespace MiddlewareDemo;

public class ExceptionHandlingMiddleware(
    RequestDelegate next,
    IProblemDetailsService problemDetailsService,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unhandled exception occurred");
            
            var (statusCode, detail) = ex switch
            {
                NoEntityFoundException => (StatusCodes.Status404NotFound, ex.Message),
                ArgumentException => (StatusCodes.Status400BadRequest, ex.Message),
                _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
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
                        ["timestamp"] = DateTime.UtcNow
                    }
                }
            });
        }
    }
}
