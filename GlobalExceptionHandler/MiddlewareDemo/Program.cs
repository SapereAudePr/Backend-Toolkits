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

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.MapGet("/error", () => { throw new NoEntityFoundException("Error Testing"); });
        app.MapGet("/bad-request", () => { throw new ArgumentException("Something was invalid"); });
        // app.MapGet("/error", () => "TEST");

        app.Use(async (context, next) =>
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = ex switch
                {
                    ArgumentException => StatusCodes.Status400BadRequest,
                    NoEntityFoundException => StatusCodes.Status404NotFound,
                    _ => StatusCodes.Status500InternalServerError
                };

                await context.Response.WriteAsync(ex.Message);
            }
        });

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.Run();
    }
}