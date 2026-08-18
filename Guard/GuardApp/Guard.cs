using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace GuardApp;

/// <summary>
/// A collection of guard-clause extension methods for validating method
/// arguments and enforcing invariants, throwing a descriptive exception
/// the moment a precondition is violated.
/// </summary>
/// <remarks>
/// Guard clauses are typically called at the very start of a constructor or
/// method — "fail fast" — so invalid state is rejected immediately at the
/// boundary, rather than surfacing later as a confusing failure somewhere
/// deep inside unrelated logic. Every method here uses
/// <see cref="CallerArgumentExpressionAttribute"/> to automatically capture
/// the caller's source expression as the parameter name, so callers don't
/// have to pass it manually (e.g. calling <c>email.CheckNullOrWhiteSpace()</c>
/// automatically reports the parameter name as <c>"email"</c>).
/// </remarks>
public static class Guard
{
    /// <summary>
    /// Ensures a string is not <see langword="null"/>, empty, or made up
    /// entirely of whitespace.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <param name="errorMessage">An optional custom error message.</param>
    /// <param name="trimValue">
    /// If <see langword="true"/>, returns the value with leading/trailing
    /// whitespace trimmed.
    /// </param>
    /// <param name="parameterName">
    /// Automatically captured from the caller's source expression; not
    /// intended to be passed explicitly.
    /// </param>
    /// <returns>The original (or trimmed) value.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="value"/> is null, empty, or whitespace-only.
    /// </exception>
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

    /// <summary>
    /// Ensures a reference-type value is not <see langword="null"/>, and
    /// optionally satisfies a custom predicate.
    /// </summary>
    /// <typeparam name="T">The reference type being checked.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="errorMessage">An optional custom error message for a null value.</param>
    /// <param name="predicate">
    /// An optional additional condition the value must satisfy once it's confirmed non-null.
    /// </param>
    /// <param name="predicateMessage">An optional custom error message if the predicate fails.</param>
    /// <param name="parameterName">
    /// Automatically captured from the caller's source expression; not
    /// intended to be passed explicitly.
    /// </param>
    /// <returns>The original value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="predicate"/> returns false.</exception>
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

    /// <summary>
    /// Ensures a string is not <see langword="null"/> or empty. Unlike
    /// <see cref="CheckNullOrWhiteSpace"/>, a string made up entirely of
    /// whitespace is considered valid here.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <param name="errorMessage">An optional custom error message.</param>
    /// <param name="trimValue">
    /// If <see langword="true"/>, returns the value with leading/trailing
    /// whitespace trimmed.
    /// </param>
    /// <param name="parameterName">
    /// Automatically captured from the caller's source expression; not
    /// intended to be passed explicitly.
    /// </param>
    /// <returns>The original (or trimmed) value.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="value"/> is null or empty.
    /// </exception>
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

    /// <summary>
    /// Ensures a value type is not equal to its <see langword="default"/> value
    /// (e.g. <c>0</c> for <see cref="int"/>, <see cref="Guid.Empty"/> for <see cref="Guid"/>).
    /// </summary>
    /// <typeparam name="T">The value type being checked.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="errorMessage">An optional custom error message.</param>
    /// <param name="parameterName">
    /// Automatically captured from the caller's source expression; not
    /// intended to be passed explicitly.
    /// </param>
    /// <returns>The original value.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="value"/> equals <see langword="default"/>.
    /// </exception>
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

    /// <summary>
    /// Ensures a collection is not <see langword="null"/> and contains at
    /// least one element.
    /// </summary>
    /// <typeparam name="T">The element type of the collection.</typeparam>
    /// <param name="collection">The collection to check.</param>
    /// <param name="errorMessage">An optional custom error message.</param>
    /// <param name="parameterName">
    /// Automatically captured from the caller's source expression; not
    /// intended to be passed explicitly.
    /// </param>
    /// <returns>The original collection.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="collection"/> is null or empty.
    /// </exception>
    /// <remarks>
    /// Calls <see cref="Enumerable.Any{T}(IEnumerable{T})"/> to check for
    /// emptiness. For a lazily-evaluated sequence (e.g. one built with
    /// <see langword="yield"/>), this causes it to be enumerated once here
    /// and again by the caller afterward — harmless for a
    /// <see cref="List{T}"/> or array, but worth knowing for sequences with
    /// side effects or expensive re-evaluation.
    /// </remarks>
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

    /// <summary>
    /// Ensures a string is not <see langword="null"/> or whitespace-only,
    /// and does not exceed a maximum allowed length.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <param name="allowedLength">The maximum allowed length, inclusive.</param>
    /// <param name="errorMessage">An optional custom error message.</param>
    /// <param name="parameterName">
    /// Automatically captured from the caller's source expression; not
    /// intended to be passed explicitly.
    /// </param>
    /// <returns>The original value, unmodified.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="value"/> is null or whitespace-only.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="value"/> is longer than <paramref name="allowedLength"/>.
    /// </exception>
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

    /// <summary>
    /// Ensures a string is not <see langword="null"/> or whitespace-only,
    /// then returns it trimmed and converted to lowercase using invariant
    /// culture rules.
    /// </summary>
    /// <param name="value">The string to normalize.</param>
    /// <param name="errorMessage">An optional custom error message.</param>
    /// <param name="parameterName">
    /// Automatically captured from the caller's source expression; not
    /// intended to be passed explicitly.
    /// </param>
    /// <returns>The trimmed, lowercased value.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="value"/> is null or whitespace-only.
    /// </exception>
    public static string NormalizeValue(
        [NotNull] this string value,
        string? errorMessage = null,
        [CallerArgumentExpression(nameof(value))]
        string? parameterName = null)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentNullException(parameterName, errorMessage ?? $"{parameterName} cannot be null or whiteSpace");

        return value.Trim().ToLowerInvariant();
    }

    /// <summary>
    /// Returns the value with leading and trailing whitespace removed.
    /// </summary>
    /// <param name="value">The string to trim.</param>
    /// <returns>The trimmed value.</returns>
    public static string TrimValue(this string value)
    {
        return value.Trim();
    }

    /// <summary>
    /// Ensures a start date does not occur after a given end date.
    /// </summary>
    /// <param name="startDate">The start date to check.</param>
    /// <param name="endDate">The end date it must not exceed.</param>
    /// <param name="errorMessage">An optional custom error message.</param>
    /// <param name="parameterName">
    /// Automatically captured from the caller's source expression; not
    /// intended to be passed explicitly.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="startDate"/> is later than <paramref name="endDate"/>.
    /// </exception>
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

    /// <summary>
    /// Ensures a <see cref="DateTimeOffset"/> does not occur in the future,
    /// relative to <see cref="DateTimeOffset.UtcNow"/>.
    /// </summary>
    /// <param name="dateTimeOffset">The value to check.</param>
    /// <param name="errorMessage">An optional custom error message.</param>
    /// <param name="parameterName">
    /// Automatically captured from the caller's source expression; not
    /// intended to be passed explicitly.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="dateTimeOffset"/> is later than the current UTC time.
    /// </exception>
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

    /// <summary>
    /// Ensures a string is a syntactically valid email address, checked
    /// against a regular expression.
    /// </summary>
    /// <param name="email">The email address to validate.</param>
    /// <param name="allowedLength">The maximum allowed length, inclusive.</param>
    /// <param name="errorMessage">An optional custom error message.</param>
    /// <param name="normalize">
    /// If <see langword="true"/>, trims and lowercases the value before validating.
    /// </param>
    /// <param name="parameterName">
    /// Automatically captured from the caller's source expression; not
    /// intended to be passed explicitly.
    /// </param>
    /// <returns>The validated (and optionally normalized) email address.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the value is null, whitespace, too long, or does not match
    /// the expected email pattern.
    /// </exception>
    /// <remarks>
    /// Faster than <see cref="ValidateEmailParsing"/> since it never throws
    /// internally to validate — but a regex can't fully capture every rule
    /// of a technically valid email address. See <see cref="ValidateEmailParsing"/>
    /// for a stricter, parser-based alternative.
    /// </remarks>
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

    /// <summary>
    /// Ensures a string is a valid email address by attempting to parse it
    /// with <see cref="System.Net.Mail.MailAddress"/>.
    /// </summary>
    /// <param name="email">The email address to validate.</param>
    /// <param name="allowedLength">The maximum allowed length, inclusive.</param>
    /// <param name="errorMessage">An optional custom error message.</param>
    /// <param name="normalize">
    /// If <see langword="true"/>, trims and lowercases the value before validating.
    /// </param>
    /// <param name="parameterName">
    /// Automatically captured from the caller's source expression; not
    /// intended to be passed explicitly.
    /// </param>
    /// <returns>The validated (and optionally normalized) email address.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the value is null, whitespace, too long, or fails to parse
    /// as a valid email address.
    /// </exception>
    /// <remarks>
    /// Stricter than <see cref="ValidateEmailRegex"/> since it relies on the
    /// framework's own parsing rules rather than a hand-written pattern, but
    /// slower, since validation happens via a caught exception internally.
    /// </remarks>
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
            var addr = new System.Net.Mail.MailAddress(email);
        }
        catch
        {
            throw new ArgumentException(
                errorMessage ?? "Invalid email address",
                parameterName);
        }

        return email;
    }

    /// <summary>
    /// Ensures an integer is not equal to zero.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="errorMessage">An optional custom error message.</param>
    /// <param name="parameterName">
    /// Automatically captured from the caller's source expression; not
    /// intended to be passed explicitly.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="value"/> is zero.
    /// </exception>
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