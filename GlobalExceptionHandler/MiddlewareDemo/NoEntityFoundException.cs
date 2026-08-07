namespace MiddlewareDemo;

/// <summary>
/// The exception that is thrown when a requested entity cannot be found.
/// </summary>
/// <param name="message">
/// The error detail that will be returned to the client in the
/// <see cref="Microsoft.AspNetCore.Mvc.ProblemDetails"/> response.
/// </param>
public class NoEntityFoundException(string message) : AppException(message)
{
    /// <inheritdoc/>
    public override int StatusCode => StatusCodes.Status404NotFound;
}