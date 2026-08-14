namespace ResultPatternApp;

public record ErrorMessage(string FieldName, string Message);

public class Result
{
    public bool IsSuccess { get; }

    public IReadOnlyCollection<ErrorMessage>? ErrorMessages { get; }

    private Result(bool isSuccess = false, List<ErrorMessage>? errorMessages = null)
    {
        IsSuccess = isSuccess;
        ErrorMessages = errorMessages ?? [];
    }

    public static Result Success() => new(true);

    public static Result Failure(string fieldName, string message) =>
        new(false, [new ErrorMessage(fieldName, message)]);
}