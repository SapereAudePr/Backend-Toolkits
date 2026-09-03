using Domain.Common;

namespace Web.Extensions;

public static class ResultExtensions
{
    public static IResult ToHttpResult<T>(this Result<T> result) =>
        result.Match(onSuccess: value => result.Status switch
            {
                ResultStatus.Deleted => Results.NoContent(),
                _ => Results.Ok(value)
            },
            onFailure: errors => result.Status switch
            {
                ResultStatus.NotFound => Results.NotFound(errors),
                ResultStatus.Conflict => Results.Conflict(errors),
                ResultStatus.ValidationFailure => Results.BadRequest(errors),
                ResultStatus.Unauthorized => Results.Unauthorized(),
                _ => Results.Problem(statusCode: 500)
            });

    public static IResult ToCreatedResult<T>(this Result<T> result, Func<T, string> locationFactory) =>
        result.Match(
            onSuccess: value => Results.Created(locationFactory(value), value),
            onFailure: errors => result.Status switch
            {
                ResultStatus.ValidationFailure => Results.BadRequest(errors),
                ResultStatus.Conflict => Results.Conflict(errors),
                _ => Results.Problem(statusCode: 500)
            });
}