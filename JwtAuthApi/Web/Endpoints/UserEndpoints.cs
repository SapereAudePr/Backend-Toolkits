using Application.Services;

namespace Web.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/user")
            .WithTags("user");

        group.MapGet("{id:int}", GetUser);
        group.MapGet("getUsers/", GetUsers);

        return app;
    }

    //TODO: Write endpoints

    private static IResult GetUsers(IUserService service)
    {
        return Results.Ok();
    }

    private static IResult GetUser(IUserService service, int id)
    {
        return Results.Ok();
    }
}