using Application.Common.Interfaces;
using Application.DTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Web.Extensions;

namespace Web.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/auth").WithTags("auth");
        group.MapPost("login", Login);
        group.MapPost("logout", async context => await Logout(context));
        group.MapGet("whoami", WhoAmI);
        group.MapPost("refresh", Refresh);
        return app;
    }

    private static async Task<IResult> Login(IAuthService service, LoginDto dto)
    {
        var result = await service.LoginAsync(dto);

        return result.ToHttpResult();
    }

    private static async Task<IResult> Logout(HttpContext context)
    {
        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return Results.Ok();
    }

    private static IResult WhoAmI(HttpContext context)
    {
        var claims = context.User.Claims.Select(c =>
            new { c.Type, c.Value });

        return Results.Ok(claims);
    }

    private static async Task<IResult> Refresh(IAuthService authService, RefreshRequestDto dto)
    {
        var result = await authService.RefreshAsync(dto);

        return result.ToHttpResult();
    }
}