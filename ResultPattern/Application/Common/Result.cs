namespace Application.Common;


/// <summary>
/// Represents an error that occurred while processing an operation.
/// </summary>
/// <param name="FieldName">
/// The name of the field or property associated with the error.
/// </param>
/// <param name="Message">
/// A descriptive message explaining the error.
/// </param>
public record ErrorMessage(string FieldName, string Message);


/// <summary>
/// Represents the result of an operation that can either succeed with a value
/// or fail with one or more error messages.
/// </summary>
/// <typeparam name="T">
/// The type of the value returned when the operation succeeds.
/// </typeparam>
public class Result<T>
{
    private T Value { get; }
    private bool IsSuccess { get; }
    private IReadOnlyList<ErrorMessage> ErrorMessages { get; }

    
    /// <summary>
    /// Initializes a new instance of the <see cref="Result{T}"/> class.
    /// </summary>
    /// <param name="value">
    /// The value returned by the successful operation.
    /// </param>
    /// <param name="isSuccess">
    /// Indicates whether the operation was successful.
    /// </param>
    /// <param name="errorMessages">
    /// The errors produced when the operation fails.
    /// </param>
    private Result(T value, bool isSuccess, List<ErrorMessage>? errorMessages = null)
    {
        Value = value;
        IsSuccess = isSuccess;
        ErrorMessages = errorMessages ?? [];
    }

    
    /// <summary>
    /// Creates a successful result containing the specified value.
    /// </summary>
    /// <param name="value">
    /// The value produced by the successful operation.
    /// </param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> containing the specified value.
    /// </returns>
    public static Result<T> Success(T value) => new(value, true);

    
    /// <summary>
    /// Creates a failed result containing the specified error messages.
    /// </summary>
    /// <param name="errorMessage">
    /// The collection of errors describing why the operation failed.
    /// </param>
    /// <returns>
    /// A failed <see cref="Result{T}"/> containing the specified errors.
    /// </returns>
    public static Result<T> Failure(IReadOnlyCollection<ErrorMessage> errorMessage) =>
        new(default!, false, errorMessage.ToList());

    
    /// <summary>
    /// Implicitly converts a value of type <typeparamref name="T"/>
    /// into a successful <see cref="Result{T}"/>.
    /// </summary>
    /// <param name="value">
    /// The value to wrap in a successful result.
    /// </param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> containing the specified value.
    /// </returns>
    public static implicit operator Result<T>(T value) => Success(value);

    
    /// <summary>
    /// Continues an operation only if the current result is successful.
    /// </summary>
    /// <typeparam name="TResult">
    /// The type of the value produced by the next operation.
    /// </typeparam>
    /// <param name="next">
    /// The next operation to execute when the current result is successful.
    /// </param>
    /// <returns>
    /// The result returned by <paramref name="next"/> when successful;
    /// otherwise, a failed result containing the existing error messages.
    /// </returns>
    /// <remarks>
    /// <see cref="Bind{TResult}"/> is useful for chaining multiple operations
    /// where each operation can fail. If the current result is a failure,
    /// the next operation is not executed and the failure is propagated.
    /// </remarks>
    public Result<TResult> Bind<TResult>(Func<T, Result<TResult>> next) =>
        IsSuccess ? next(Value) : Result<TResult>.Failure(ErrorMessages);

    
    /// <summary>
    /// Transforms the result into another value depending on whether
    /// the operation succeeded or failed.
    /// </summary>
    /// <typeparam name="TResult">
    /// The type of value returned by either handler.
    /// </typeparam>
    /// <param name="onSuccess">
    /// The function executed when the result is successful.
    /// </param>
    /// <param name="onFailure">
    /// The function executed when the result is a failure.
    /// </param>
    /// <returns>
    /// The value returned by either <paramref name="onSuccess"/>
    /// or <paramref name="onFailure"/>.
    /// </returns>
    public TResult Match<TResult>
    (Func<T, TResult> onSuccess,
        Func<IReadOnlyCollection<ErrorMessage>, TResult> onFailure) =>
        IsSuccess ? onSuccess(Value) : onFailure(ErrorMessages);

    
    /// <summary>
    /// Returns a string representation of the result.
    /// </summary>
    /// <returns>
    /// A string describing whether the result was successful or failed,
    /// including its value or error messages.
    /// </returns>
    public override string ToString() =>
        IsSuccess ? $"Success: {Value}" : $"Failure: {string.Join("; ", ErrorMessages)}";
}