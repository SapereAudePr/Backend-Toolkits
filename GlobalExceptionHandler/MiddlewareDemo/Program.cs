using System.Net.Mime;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace MiddlewareDemo;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddAuthorization();

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        builder.Services.AddProblemDetails();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }


        // app.UseMiddleware<ExceptionHandlingMiddleware>();

        app.UseExceptionHandler(exceptionHandlerApp =>
        {
            exceptionHandlerApp.Run(async context =>
            {
                var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

                var problemsDetailsService = context.RequestServices.GetRequiredService<IProblemDetailsService>();

                var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();

                var ex = exceptionHandlerPathFeature?.Error;

                var errorMessage = ex?.Message;

                logger.LogError(ex, "Unhandled exception occurred");

                var (statusCode, detail) = ex switch
                {
                    ArgumentException =>
                        (StatusCodes.Status400BadRequest, errorMessage),
                    NoEntityFoundException =>
                        (StatusCodes.Status404NotFound, errorMessage),
                    FileNotFoundException =>
                        (StatusCodes.Status404NotFound, "The file not found"),
                    _ =>
                        (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
                };

                context.Response.StatusCode = statusCode;

                await problemsDetailsService.WriteAsync(new ProblemDetailsContext
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

                if (exceptionHandlerPathFeature?.Path == "/")
                    await context.Response.WriteAsync(" Page: Home.");
            });
        });

        app.UseHttpsRedirection();

        app.UseAuthorization();


        app.MapGet("/not-found", () => { throw new NoEntityFoundException("Something was not found"); });
        app.MapGet("/bad-request", () => { throw new ArgumentException("Something was invalid"); });
        app.MapGet("server-error", () => { throw new Exception(); });
        app.MapGet("/success", () => "TEST");


        app.Run();
    }
}