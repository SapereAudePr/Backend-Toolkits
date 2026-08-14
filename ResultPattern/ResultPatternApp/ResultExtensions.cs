namespace ResultPatternApp;

public static class ResultExtensions
{
    public static void CheckResult(this Result result)
    {
        if (!result.IsSuccess)
            Console.WriteLine(string.Join(Environment.NewLine, result.ErrorMessages!));
    }
}