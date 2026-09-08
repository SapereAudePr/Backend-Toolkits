using Application.Common;
using Application.Common.Interfaces;
using Application.DTOs;
using Application.Validation.Validate;
using Domain.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
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

    private static async Task<IResult> Login(IAuthService service,
        ITokenService tokenService, IApplicationDbContext dbContext,
        LoginDto dto)
    {
        var result = await service.Login(dto);

        var user = result.Match(onSuccess: userDto => userDto, onFailure:
            _ => (UserDto?)null);
        if (user is null)
            return result.ToHttpResult();

        var accessToken = tokenService.GenerateAccessToken(user);
        var rawRefreshToken = tokenService.GenerateRefreshToken();
        var hashedRefreshToken = tokenService.HashToken(rawRefreshToken);

        var refreshToken = new RefreshToken(
            hashedRefreshToken, user.Id, expiresAt: DateTimeOffset.UtcNow.AddDays(7));

        await dbContext.RefreshTokens.AddAsync(refreshToken);
        await dbContext.SaveChangesAsync();

        return Results.Ok(new AuthResponseDto
            { AccessToken = accessToken, RefreshToken = rawRefreshToken });
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


    //TODO:
    // There's no protection for concurrency token(double-fired, token send at the same time)
    private static async Task<IResult> Refresh(ITokenService service, IApplicationDbContext dbContext,
        RefreshRequestDto dto)
    {
        var validateToken = RefreshTokenValidation.ValidateRefreshToken(dto);
        if (!validateToken.IsValid)
            return Results.Unauthorized();

        var hashedToken = service.HashToken(dto.RefreshToken);

        var storedToken = await dbContext.RefreshTokens.FirstOrDefaultAsync(x =>
            x.TokenHash == hashedToken);

        if (storedToken is null)
            return Results.Unauthorized();

        if (storedToken.RevokedAt is not null)
        {
            var tokens = await dbContext.RefreshTokens.Where(t =>
                    t.UserId == storedToken.UserId && t.RevokedAt == null)
                .ToListAsync();

            foreach (var t in tokens)
                t.Revoke();

            await dbContext.SaveChangesAsync();

            return Results.Unauthorized();
        }

        if (!storedToken.IsActive)
            return Results.Unauthorized();

        var user = await dbContext.Users.AsNoTracking().Where(u =>
                u.Id == storedToken.UserId)
            .Select(u => new UserDto { Id = u.Id, Name = u.Name })
            .FirstOrDefaultAsync();

        if (user is null)
            return Results.Unauthorized();

        storedToken.Revoke();

        var accessToken = service.GenerateAccessToken(user);
        var rawRefreshToken = service.GenerateRefreshToken();
        var hashedRefreshToken = service.HashToken(rawRefreshToken);

        var newRefreshToken = new RefreshToken(hashedRefreshToken, user.Id,
            DateTimeOffset.UtcNow.AddDays(7));

        await dbContext.RefreshTokens.AddAsync(newRefreshToken);
        await dbContext.SaveChangesAsync();

        return Results.Ok(new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = rawRefreshToken
        });
    }
}