using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Application.Common;

public static class Guard
{
    public static string CheckNullOrWhiteSpace(
        [NotNull] this string value,
        string? errorMessage = null,
        bool trimValue = false,
        [CallerArgumentExpression(nameof(value))]
        string? parameterName = null)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException(errorMessage ?? $"{parameterName} cannot be null or whiteSpace", parameterName);

        return trimValue ? value.Trim() : value;
    }

    public static T CheckNull<T>(
        [NotNull] this T value,
        string? errorMessage = null,
        Func<T, bool>? predicate = null,
        string? predicateMessage = null,
        [CallerArgumentExpression(nameof(value))]
        string? parameterName = null)
        where T : class
    {
        if (value is null)
            throw new ArgumentNullException
                (parameterName, errorMessage ?? $"{parameterName} cannot be null");

        if (predicate is not null && !predicate(value))
            throw new ArgumentException(predicateMessage ?? $"Validation failed for {parameterName}");

        return value;
    }

    public static string MinLength(
        [NotNull] this string value,
        int min,
        string? errorMessage = null,
        [CallerArgumentExpression(nameof(value))]
        string? parameterName = null)
    {
        value.CheckNull(errorMessage, parameterName: parameterName);

        return value.Length >= min
            ? value
            : throw new ArgumentException(
                errorMessage ?? $"{parameterName} length ({value.Length}) cannot be lower than {min}",
                parameterName);
    }


    public static string MaxLength(
        [NotNull] this string value,
        int max,
        string? errorMessage = null,
        [CallerArgumentExpression(nameof(value))]
        string? parameterName = null)
    {
        value.CheckNull(errorMessage, parameterName: parameterName);

        return value.Length <= max
            ? value
            : throw new ArgumentException(
                errorMessage ?? $"{parameterName} length ({value.Length}) cannot be higher than {max}",
                parameterName);
    }


    public static string CheckNullOrEmpty(
        [NotNull] this string value,
        string? errorMessage = null,
        bool trimValue = false,
        [CallerArgumentExpression(nameof(value))]
        string? parameterName = null)
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentException(errorMessage ?? $"{parameterName} cannot be null or empty", parameterName);

        return trimValue ? value.Trim() : value;
    }

    public static T CheckNotDefault<T>(
        this T value,
        string? errorMessage = null,
        [CallerArgumentExpression(nameof(value))]
        string? parameterName = null)
        where T : struct
    {
        if (EqualityComparer<T>.Default.Equals(value, default))
            throw new ArgumentException(errorMessage ?? $"{parameterName} cannot be default", parameterName);

        return value;
    }

    public static IEnumerable<T> CheckNotEmpty<T>(
        this IEnumerable<T>? collection,
        string? errorMessage = null,
        [CallerArgumentExpression(nameof(collection))]
        string? parameterName = null)
    {
        if (collection is null || !collection.Any())
            throw new ArgumentException(errorMessage ?? $"{parameterName} cannot be null or empty", parameterName);

        return collection;
    }

    public static string CheckTooLongOrEmpty(
        [NotNull] this string value,
        int allowedLength,
        string? errorMessage = null,
        [CallerArgumentExpression(nameof(value))]
        string? parameterName = null)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException(errorMessage ?? $"{parameterName} cannot be null or whiteSpace", parameterName);
        if (value.Length > allowedLength)
            throw new ArgumentOutOfRangeException
            (parameterName, value.Length,
                errorMessage ?? $"{parameterName} cannot exceed {allowedLength} characters");

        return value;
    }

    public static string NormalizeValue(
        [NotNull] this string value,
        string? errorMessage = null,
        [CallerArgumentExpression(nameof(value))]
        string? parameterName = null)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentNullException(parameterName,
                errorMessage ?? $"{parameterName} cannot be null or whiteSpace");

        return value.Trim().ToLowerInvariant();
    }

    public static string TrimValue(this string value)
    {
        return value.Trim();
    }

    public static void CheckStartHigherThanEnd(
        this DateTime startDate,
        DateTime endDate,
        string? errorMessage = null,
        [CallerArgumentExpression(nameof(startDate))]
        string? parameterName = null)
    {
        if (startDate > endDate)
            throw new ArgumentOutOfRangeException(
                parameterName, startDate, errorMessage ?? $"{parameterName} cannot exceed {endDate}");
    }

    public static void CheckCreationDateTimeOffset(
        this DateTimeOffset dateTimeOffset,
        string? errorMessage = null,
        [CallerArgumentExpression(nameof(dateTimeOffset))]
        string? parameterName = null)
    {
        if (dateTimeOffset > DateTimeOffset.UtcNow)
            throw new ArgumentOutOfRangeException(
                parameterName, dateTimeOffset, errorMessage ?? $"{parameterName} cannot be in future");
    }

    private static readonly Regex EmailRegex = new Regex
        (@"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);


    public static string ValidateEmailRegex(
        [NotNull] this string email,
        int allowedLength = 254,
        string? errorMessage = null,
        bool normalize = false,
        [CallerArgumentExpression(nameof(email))]
        string? parameterName = null)
    {
        email = normalize ? email.NormalizeValue() : email;
        email.CheckTooLongOrEmpty(allowedLength, errorMessage, parameterName);

        if (!EmailRegex.IsMatch(email))
            throw new ArgumentException(errorMessage ?? $"Invalid email address", parameterName);

        return email;
    }

    public static string ValidateEmailParsing(
        [NotNull] this string email,
        int allowedLength = 254,
        string? errorMessage = null,
        bool normalize = false,
        [CallerArgumentExpression(nameof(email))]
        string? parameterName = null)
    {
        email.CheckTooLongOrEmpty(allowedLength, errorMessage, parameterName);

        email = normalize ? email.NormalizeValue() : email;

        try
        {
            _ = new System.Net.Mail.MailAddress(email);
        }
        catch
        {
            throw new ArgumentException(
                errorMessage ?? "Invalid email address",
                parameterName);
        }

        return email;
    }

    public static void CheckIfZero(
        this int value,
        string? errorMessage = null,
        [CallerArgumentExpression(nameof(value))]
        string? parameterName = null)
    {
        if (value == 0)
            throw new ArgumentException(errorMessage ?? $"{parameterName} cannot be zero", parameterName);
    }
}