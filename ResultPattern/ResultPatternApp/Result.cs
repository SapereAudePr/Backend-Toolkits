namespace ResultPatternApp;

public record ErrorMessage(string FieldName, string Message);

public class Result<T>
{
    private T Value { get; }

    private bool IsSuccess { get; }

    private IReadOnlyCollection<ErrorMessage>? ErrorMessages { get; }

    private Result(T value, List<ErrorMessage>? errorMessages = null, bool isSuccess = false)
    {
        Value = value;
        IsSuccess = isSuccess;
        ErrorMessages = errorMessages ?? [];
    }

    public static Result<T> Success(T value) => new(value, isSuccess: true);

    public static Result<T> Failure(IReadOnlyCollection<ErrorMessage> errorMessages) =>
        new(default!, errorMessages.ToList(), false);

    public Result<TResult> Bind<TResult>(Func<T, Result<TResult>> next) =>
        !IsSuccess ? Result<TResult>.Failure(ErrorMessages!) : next(Value);

    public TResult Match<TResult>(Func<T, TResult> onSuccess,
        Func<IReadOnlyCollection<ErrorMessage>, TResult> onFailure) =>
        !IsSuccess ? onFailure(ErrorMessages!) : onSuccess(Value);
}