using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace MiddlewareDemo;

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


            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            var details = new ProblemDetails
            {
                Detail = ex.Message,
                Status = statusCode,
                Title = ReasonPhrases.GetReasonPhrase(statusCode),
                Type = $"https://httpstatuses.com/{statusCode}",
                Instance = context.Request.Path,
                Extensions =
                {
                    ["traceId"] = context.TraceIdentifier,
                    ["timestamp"] = DateTime.UtcNow
                }
            };

            await context.Response.WriteAsJsonAsync(details, options);
        }
    }
}