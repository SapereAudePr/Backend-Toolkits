namespace Domain.Common;

public record ErrorMessage(string FieldName, string Message);

public class Result<T>
{
    private T Value { get; }

    private bool IsSuccess { get; }

    private IReadOnlyCollection<ErrorMessage>? ErrorMessages { get; }

    public ResultStatus Status { get; }

    private Result(T value, ResultStatus status, List<ErrorMessage>? errorMessages = null, bool isSuccess = false)
    {
        Value = value;
        IsSuccess = isSuccess;
        ErrorMessages = errorMessages ?? [];
        Status = status;
    }

    public static Result<T> Success(
        T value, ResultStatus status = ResultStatus.Ok) => new(value, status, isSuccess: true);

    public static Result<T> Failure(
        IReadOnlyCollection<ErrorMessage> errorMessages, ResultStatus status) =>
        new(default!, status, errorMessages.ToList(), false);

    public Result<TResult> Bind<TResult>(Func<T, Result<TResult>> next) =>
        !IsSuccess ? Result<TResult>.Failure(ErrorMessages!, Status) : next(Value);

    public TResult Match<TResult>(Func<T, TResult> onSuccess,
        Func<IReadOnlyCollection<ErrorMessage>, TResult> onFailure) =>
        !IsSuccess ? onFailure(ErrorMessages!) : onSuccess(Value);
}

public enum ResultStatus
{
    Ok,
    ValidationFailure,
    NotFound,
    Conflict,
    Deleted,
    Unauthorized
}