# Guard

A set of extension-method guard clauses for validating method arguments and
enforcing invariants — "fail fast," throwing a descriptive exception the
moment a precondition is violated, rather than letting invalid state travel
deeper into the codebase before surfacing as a confusing failure elsewhere.

## The problem this solves

Constructors and methods that accept external input need to reject invalid
values immediately. Doing this with inline `if` statements everywhere is
repetitive and easy to get subtly wrong (see "Known issues fixed" below for
a real example). `Guard` centralizes these checks as reusable, chainable
extension methods, so the calling code stays a single readable line.

## Usage

```csharp
public class Order
{
    public string CustomerEmail { get; }
    public int Quantity { get; }

    public Order(string customerEmail, int quantity)
    {
        CustomerEmail = customerEmail.ValidateEmailRegex(normalize: true);
        Quantity = quantity.CheckIfZero() is var _ ? quantity : quantity; // CheckIfZero just validates
    }
}
```

Guard clauses read naturally at a call site because they're extension methods
on the value being checked:

```csharp
name.CheckNullOrWhiteSpace();
age.CheckNotDefault();
email.ValidateEmailRegex(normalize: true);
startDate.CheckStartHigherThanEnd(endDate);
```

## Available guards

| Method | Applies to | Description |
|---|---|---|
| `CheckNullOrWhiteSpace()` | `string` | Fails if null, empty, or whitespace-only. |
| `CheckNullOrEmpty()` | `string` | Fails if null or empty. Whitespace-only strings are considered valid. |
| `CheckNull()` | any reference type | Fails if null, with an optional additional predicate. |
| `CheckNotDefault()` | any value type | Fails if equal to the type's `default` value. |
| `CheckNotEmpty()` | `IEnumerable<T>` | Fails if null or contains no elements. |
| `CheckTooLongOrEmpty(int)` | `string` | Fails if null/whitespace, or longer than the given length. |
| `NormalizeValue()` | `string` | Fails if null/whitespace; otherwise trims and lowercases. |
| `TrimValue()` | `string` | Trims whitespace. Does not validate anything. |
| `CheckStartHigherThanEnd(DateTime)` | `DateTime` | Fails if the start date is after the end date. |
| `CheckCreationDateTimeOffset()` | `DateTimeOffset` | Fails if the value is in the future. |
| `ValidateEmailRegex()` | `string` | Fails if not a valid email address, checked via regex. |
| `ValidateEmailParsing()` | `string` | Fails if not a valid email address, checked via `MailAddress` parsing. |
| `CheckIfZero()` | `int` | Fails if the value is zero. |

## Architecture

- Every method is a **`this`-prefixed extension method**, so guards read as
  part of the value being checked (`value.CheckNullOrWhiteSpace()`) rather
  than as a static utility call (`Guard.CheckNullOrWhiteSpace(value)`).
- **`[CallerArgumentExpression(nameof(value))]`** automatically captures the
  literal source expression passed as `value` and uses it as the reported
  parameter name — the caller never has to pass it manually. Calling
  `email.CheckNullOrWhiteSpace()` automatically reports `"email"` as the
  parameter name in the resulting exception, even though nothing was
  explicitly passed for it.
- **`[NotNull]`** (from `System.Diagnostics.CodeAnalysis`) is a static-analysis
  hint, not a runtime check — it tells the compiler's nullable reference
  analysis that after this method returns normally, the value is guaranteed
  non-null, which silences unnecessary null-warnings at the call site.
- Most guards throw `ArgumentException`/`ArgumentNullException`/`ArgumentOutOfRangeException`
  — all standard, BCL-recognized exception types — rather than a custom
  exception hierarchy, since a guard clause failing usually represents a
  genuine programming/contract violation (see "Where this fits" below).

## Where this fits alongside this repo's other patterns

- **`Guard` vs `Validator`.** `Validator<T>` is for *user-facing input*:
  collect every problem across a whole object, report them all together,
  never throw. `Guard` is for *enforcing invariants at a boundary* — usually
  inside a constructor or a method that should never have been called with
  bad arguments in the first place. A `Guard` failure represents a
  programming mistake (a caller violating a contract), not routine user input.
- **`Guard` vs Global Exception Handling.** The exceptions `Guard` throws
  (`ArgumentException` and its subtypes) are exactly the category of
  exception this repo's `FallbackExceptionHandler` is built to catch — not
  `AppExceptionHandler`. That's intentional: a `Guard` failure means *your
  own code* passed a bad value, which is a bug, not an expected business
  outcome, so it should surface as a generic 500 rather than a friendly,
  specific client-facing error.
- **Unlike `Result<T>`**, `Guard` clauses always throw rather than returning
  a failure value. This is deliberate — guard clauses exist specifically for
  conditions that should be *impossible* if the rest of the codebase is
  correct, not for expected, routine "no" outcomes.

## Known issues fixed

- **`NormalizeValue`** originally called `new ArgumentNullException(errorMessage ?? ...)`
  — `ArgumentNullException`'s single-string constructor sets `ParamName`, not
  `Message`. This meant the exception's `ParamName` ended up being a full
  sentence instead of the actual parameter name, and the intended custom
  message never actually surfaced. Fixed to
  `new ArgumentNullException(parameterName, errorMessage ?? ...)`, matching
  the two-argument constructor's `(paramName, message)` order.

## Design notes worth knowing about, not yet resolved

- **`CheckNullOrWhiteSpace` vs `CheckNullOrEmpty`** overlap significantly —
  the only difference is whether a whitespace-only string is considered
  valid. Both are kept since call sites genuinely differ on which behavior
  they want (e.g. a name should probably reject whitespace-only input, while
  some raw/preformatted text field might not need to).
- **`ValidateEmailRegex` vs `ValidateEmailParsing`** are two independent
  implementations of the same goal, kept side by side deliberately: regex is
  faster and avoids exception-based control flow, while `MailAddress`
  parsing is stricter and defers to the framework's own understanding of
  what counts as a valid address, at the cost of relying on a caught
  exception internally to detect invalid input.
- **`CheckNotEmpty<T>`** enumerates the given `IEnumerable<T>` once via
  `.Any()` to check for emptiness, then returns it for the caller to
  enumerate again. Harmless for a `List<T>` or array, but worth knowing if
  it's ever called with a lazily-evaluated sequence (e.g. built with
  `yield return`) that has side effects or is expensive to re-evaluate.