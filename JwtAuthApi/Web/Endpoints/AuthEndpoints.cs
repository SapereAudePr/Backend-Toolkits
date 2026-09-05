using System.Security.Claims;
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
        return app;
    }

    private static async Task<IResult> Login(IAuthService service,
        ITokenService tokenService, LoginDto dto)
    {
        var result = await service.Login(dto);

        var user = result.Match(onSuccess: userDto => userDto, onFailure:
            _ => (UserDto?)null);
        if (user is null)
            return result.ToHttpResult();

        var token = tokenService.GenerateToken(user);

        return Results.Ok(new LoginResponseDto{Token = token});
    }
    
    private static async Task<IResult> Logout(HttpContext context)
    {
        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return Results.Ok();
    }

    private static IResult WhoAmI(HttpContext context)
    {
        var claims = context.User.Claims.Select(c => new { c.Type, c.Value });

        return Results.Ok(claims);
    }
}