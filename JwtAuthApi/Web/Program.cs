using Application.Common;
using Application.Common.Interfaces;
using Application.Security;
using Application.Services;
using Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Web.Endpoints;
using Web.Middleware;

namespace Web;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddAuthorization();

        builder.Services.AddOpenApi();

        builder.Services.AddDbContext<UserDbContext>(options =>
            options.UseSqlServer(
                builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<UserDbContext>());

        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();

        builder.Services.AddScoped<IUserService, UserService>();

        builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseExceptionHandler();

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapUserEndpoints();

        app.Run();
    }
}