using Application.Common.Interfaces;
using Application.DTOs;
using Web.Extensions;

namespace Web.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/auth").WithTags("auth");
        group.MapPost("login", Login);
        return app;
    }

    private static async Task<IResult> Login(IAuthService service, LoginDto dto)
    {
        var result = await service.Login(dto);

        return result.ToHttpResult();
    }
}