namespace MiddlewareDemo;

/// <summary>
/// Base type for exceptions that represent expected, application-specific failures
/// (as opposed to unanticipated bugs or infrastructure errors).
/// </summary>
/// <remarks>
/// Centralizing <see cref="StatusCode"/> here lets <see cref="AppExceptionHandler"/>
/// handle any current or future subtype with a single type check, instead of
/// switching on each concrete exception type individually.
/// </remarks>
/// <param name="message">The error detail, exposed to the client via ProblemDetails.</param>
public abstract class AppException(string message) : Exception(message)
{
    /// <summary>
    /// The HTTP status code that should be returned to the client for this exception.
    /// </summary>
    public abstract int StatusCode { get; }
}