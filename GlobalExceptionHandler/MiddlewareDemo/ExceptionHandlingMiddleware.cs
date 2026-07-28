using System.Text.Json;

namespace MiddlewareDemo;

public record ErrorResponse(string Message, int StatusCode, DateTimeOffset TimeStamp);

public class ExceptionHandlingMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var statusCode = ex switch
            {
                NoEntityFoundException => StatusCodes.Status404NotFound,
                ArgumentException => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            };

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var response = new ErrorResponse(
                ex.Message,
                statusCode,
                DateTime.UtcNow
            );

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            await context.Response.WriteAsJsonAsync(response, options);
        }
    }
}