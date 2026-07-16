using System.Text.RegularExpressions;

namespace Validation;

public record ValidationError(string Field, string Message);

public record ValidationResult(bool IsValid, IReadOnlyList<ValidationError> Errors)
{
    public static ValidationResult Success() =>
        new(true, []);
}

/// <summary>
/// Provides the entry point for building a chain of validation rules for
/// <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of object to validate.</typeparam>
/// <param name="subject">The object instance to validate.</param>
/// <example>
/// <code>
/// var result = new Validator&lt;Person&gt;(new Person("John", 25))
///     .RuleFor("Name", person => person.Name)
///     .NotNull()
///     .MinLength(2)
///     .RuleFor("Age", person => person.Age)
///     .Min(1)
///     .GreaterThan(12)
///     .Validate();
/// </code>
/// </example>
public class Validator<T>(T subject)
{
    private readonly List<ValidationError> _errors = [];

    public void AddError(string field, string message) =>
        _errors.Add(new ValidationError(field, message));

    public PropertyValidator<T, TProp> RuleFor<TProp>(string field, Func<T, TProp> selector) =>
        new(this, field, selector(subject));

    public ValidationResult Validate() =>
        _errors.Count == 0 ? ValidationResult.Success() : new ValidationResult(false, _errors);
}

/// <summary>
/// 
/// </summary>
/// <remarks>
/// Rules can only work for two types: <see cref="string"/> or <see cref="IComparable{T}"/>
/// while other types than those simply no-op
/// </remarks>
/// <param name="parent"></param>
/// <param name="field"></param>
/// <param name="value"></param>
/// <typeparam name="T"></typeparam>
/// <typeparam name="TProp"></typeparam>
public class PropertyValidator<T, TProp>(Validator<T> parent, string field, TProp value)
{
    public PropertyValidator<T, TProp> NotEmpty()
    {
        if (value is string s && string.IsNullOrEmpty(s))
            parent.AddError(field, $"{field} can not be empty.");

        return this;
    }

    public PropertyValidator<T, TProp> NotNull()
    {
        if (value is null)
            parent.AddError(field, $"{field} can not be null.");

        return this;
    }

    public PropertyValidator<T, TProp> MinLength(int min)
    {
        if (value is string s && s.Length < min)
            parent.AddError(field, $"{field} can not be lower than {min}");

        return this;
    }

    public PropertyValidator<T, TProp> MaxLength(int max)
    {
        if (value is string s && s.Length > max)
            parent.AddError(field, $"{field} can not be higher than {max}");

        return this;
    }

    /// <summary>
    /// Validates a value against a minimum threshold using <see cref="IComparable{T}"/>.
    /// Adds a validation error if the value is lower than <paramref name="min"/>.
    /// </summary>
    /// <param name="min">The minimum allowed value, inclusive.</param>
    /// <returns>The current <see cref="PropertyValidator{T, TProp}"/> instance, for chaining.</returns>
    public PropertyValidator<T, TProp> Min(TProp min)
    {
        if (value is IComparable<TProp> comparable && comparable.CompareTo(min) < 0)
            parent.AddError(field, $"{field} can not be lower than {min}");

        return this;
    }

    public PropertyValidator<T, TProp> Max(TProp max)
    {
        if (value is IComparable<TProp> comparable && comparable.CompareTo(max) > 0)
            parent.AddError(field, $"{field} can not be higher than {max}");

        return this;
    }

    public PropertyValidator<T, TProp> LessThan(TProp flagNumber)
    {
        if (value is IComparable<TProp> comparable && comparable.CompareTo(flagNumber) >= 0)
            parent.AddError(field, $"{field} must be lower than {flagNumber}");

        return this;
    }

    public PropertyValidator<T, TProp> GreaterThan(TProp flagNumber)
    {
        if (value is IComparable<TProp> comparable && comparable.CompareTo(flagNumber) <= 0)
            parent.AddError(field, $"{field} must be greater than {flagNumber}");

        return this;
    }

    public PropertyValidator<T, TProp> Equal(TProp expected)
    {
        if (!EqualityComparer<TProp>.Default.Equals(value, expected))
            parent.AddError(field, $"{field} must be equal to {expected}");

        return this;
    }

    public PropertyValidator<T, TProp> NotEqual(TProp notExpected)
    {
        if (EqualityComparer<TProp>.Default.Equals(value, notExpected))
            parent.AddError(field, $"{field} must not be {notExpected}");

        return this;
    }

    public PropertyValidator<T, TProp> Must(Func<TProp, bool> rule, string? errorDescription = null)
    {
        if (!rule(value))
            parent.AddError(field, errorDescription ?? $"{field} doesn't match the rules");

        return this;
    }

    public PropertyValidator<T, TProp> EmailAddress()
    {
        var pattern = @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$";

        if (value is string s && !Regex.IsMatch(s, pattern))
            parent.AddError(field, $"{field} is not a valid email address");

        return this;
    }

    public PropertyValidator<T, TProp> Matches(string pattern, string? errorMessage = null)
    {
        if (value is string s && !Regex.IsMatch(s, pattern))
            parent.AddError(field, errorMessage ?? $"{field} doesn't match required pattern");

        return this;
    }

    public PropertyValidator<T, TNextProp> RuleFor<TNextProp>(string fieldName, Func<T, TNextProp> selector) =>
        parent.RuleFor(fieldName, selector);

    public ValidationResult Validate() =>
        parent.Validate();
}