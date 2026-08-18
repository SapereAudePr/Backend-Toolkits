# Validator

A fluent, chainable validation library built from scratch — inspired by
FluentValidation — to understand how a fluent builder API and generic
constraints actually work under the hood, rather than just consuming one.

## The problem this solves

Validation logic scattered across `if` statements makes it easy to stop at the
first failure and miss everything else wrong with the input, and it tends to be
duplicated differently in every method that needs it. `Validator<T>` collects
every rule violation across every field in one pass and returns them together,
so a caller (or an API response) can show a user everything wrong with their
input at once, not one field at a time across multiple round trips.

## Usage

```csharp
var result = new Validator<Person>(new Person("John", 25))
    .RuleFor("Name", x => x.Name)
        .NotEmpty()
        .MinLength(2)
        .MaxLength(50)
    .RuleFor("Age", x => x.Age)
        .Min(0)
        .Max(120)
    .Validate();

if (!result.IsValid)
{
    foreach (var error in result.Errors)
        Console.WriteLine($"{error.Field}: {error.Message}");
}
```

## Available rules

| Method | Applies to | Description |
|---|---|---|
| `NotNull()` | any nullable type | Fails if the value is `null`. |
| `NotEmpty()` | `string` | Fails if the value is `null` or empty. No-ops for non-string types. |
| `MinLength(int)` / `MaxLength(int)` | `string` | Fails if the string is shorter/longer than the given length. No-ops for non-string types. |
| `Min(TProp)` / `Max(TProp)` | `IComparable<T>` types | Fails if the value is lower/higher than the given bound, inclusive. No-ops for non-comparable types. |
| `LessThan(TProp)` / `GreaterThan(TProp)` | `IComparable<T>` types | Fails if the value isn't strictly under/over the given bound. No-ops for non-comparable types. |
| `Equal(TProp)` / `NotEqual(TProp)` | any | Fails based on the default equality comparer for the type. |
| `Must(Func<TProp, bool>, string?)` | any | Fails if the given predicate returns `false`. Escape hatch for any rule not covered above. |
| `EmailAddress()` | `string` | Fails if the value doesn't match a basic email pattern. No-ops for non-string types. |
| `Matches(string pattern, string?)` | `string` | Fails if the value doesn't match the given regular expression. No-ops for non-string types. |

## Architecture

- **`Validator<T>`** is the entry point. `RuleFor(field, selector)` extracts a
  single property's value from the subject and hands back a `PropertyValidator`
  scoped to that field, so rules can be chained directly onto it.
- **`PropertyValidator<T, TProp>`** holds the chainable rule methods for a
  single field. Every rule method returns `this`, which is what makes chaining
  possible, and calling `RuleFor` again on it moves on to the next field
  without needing to go back through `Validator<T>` explicitly.
- **`ValidationResult`** is the final output — an `IsValid` flag plus every
  `ValidationError` collected across every field, not just the first failure.
- Rules that only make sense for a specific runtime type (string-based rules,
  comparison rules) use a runtime `is` check rather than a generic constraint
  on `TProp`. This keeps `RuleFor<TProp>` usable for *any* property type —
  rules that don't apply to a given type simply no-op instead of causing a
  compile error on the call to `RuleFor` itself.

## Design notes

- **Why runtime checks instead of a generic constraint.** An earlier version
  constrained `PropertyValidator<T, TProp>` with `where TProp : IComparable<TProp>`
  at the class level. That made *every* rule — even `NotNull()` — require a
  comparable type, which broke validation for ordinary reference types with no
  natural ordering. Moving the check inside each individual method
  (`value is IComparable<TProp> comparable`) keeps the class usable for any
  `TProp`, and only the rules that genuinely need comparability enforce it,
  silently no-opping otherwise.
- **Why the constructor takes the subject directly, not a factory.**
  `Validator<T>(T subject)` extracts each field's value once, up front, via
  `RuleFor`'s selector — rules then operate on that already-captured value
  rather than re-reading the subject on every call.
- **Deliberately left out of this implementation:**
  - **Custom global error messages / localization** — every rule builds its
    own message inline. A message-template system would be a natural addition
    for an app that needs multi-language validation output.
  - **Async rules** — useful for checks that require I/O (e.g. "is this email
    already registered"), which don't fit this library's current synchronous
    `Func<TProp, bool>` shape for `Must`. Left out since nothing in this repo
    currently needs it.
  - **Nested object validation** (validating a property that is itself a
    complex object with its own rules) — not needed by the current use cases,
    but a common extension point in real validation libraries.