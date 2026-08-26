using Application.DTOs;
using Application.Services;
using Web.Extensions;

namespace Web.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/user")
            .WithTags("user");

        group.MapGet("", GetUsers);
        group.MapGet("{id:int}", GetUser);
        group.MapPost("", CreateUser);
        group.MapDelete("{id:int}", DeleteUser);

        return app;
    }

    //TODO: Write endpoints

    private static async Task<IResult> GetUsers(IUserService service)
    {
        var users = await service.GetUsers();

        return users.ToHttpResult();
    }

    private static async Task<IResult> GetUser(IUserService service, int id)
    {
        var user = await service.GetUser(id);

        return user.ToHttpResult();
    }

    private static async Task<IResult> CreateUser(IUserService service, CreateUserDto userDto)
    {
        var user = await service.CreateUser(userDto);

        return user.ToCreatedResult(x => $"/api/user/{x.Id}");
    }

    private static async Task<IResult> DeleteUser(IUserService service, int id)
    {
        var user = await service.DeleteUser(id);

        return user.ToHttpResult();
    }
}