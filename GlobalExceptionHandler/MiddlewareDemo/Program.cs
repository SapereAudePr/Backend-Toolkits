namespace MiddlewareDemo;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // It won't be invoked since not injected to the DependencyContainer
        // I kept it to reference as an alternative version
        // builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

        // Order matters. Register the fallback handler last;
        // otherwise, every exception will be handled as a 500 Internal Server Error.
        builder.Services.AddExceptionHandler<AppExceptionHandler>();
        builder.Services.AddExceptionHandler<FallbackExceptionHandler>();


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

        app.UseExceptionHandler();

        app.UseHttpsRedirection();

        app.UseAuthorization();


        app.MapGet("/not-found", () => { throw new NoEntityFoundException("Something was not found"); });
        app.MapGet("/bad-request", () => { throw new ArgumentException("Something was invalid"); });
        app.MapGet("server-error", () => { throw new Exception(); });
        app.MapGet("/success", () => "TEST");


        app.Run();
    }
}