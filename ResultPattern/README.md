# Result Pattern

A generic `Result<T>` built from scratch to represent the outcome of an operation
that can expectedly fail — without throwing, and without falling back to `bool`
or `null`, both of which lose the *reason* something failed.

## The problem this solves

`bool` tells a caller *that* something failed, but not *why*. `null` is ambiguous —
does it mean "not found," or did something actually go wrong? Throwing an exception
for a routine, expected outcome (insufficient balance, a duplicate ID, invalid input)
is expensive and semantically wrong — exceptions should represent the unexpected,
not a normal "no" from a business rule.

`Result<T>` represents either success (with a value) or failure (with one or more
structured error messages), and its constructor is deliberately locked down so an
invalid, self-contradictory state — success with an error, or failure with no
message — can never be constructed at all.

## Usage

```csharp
public Result<Account> GetAccount(int id)
{
    var account = _accounts.FirstOrDefault(x => x.Id == id);

    if (account is null)
        return Result<Account>.Failure([
            new ErrorMessage("id", $"Account with id {id} was not found.")
        ]);

    return account; // implicit conversion to Result<Account>.Success(account)
}
```

Chaining multiple operations that can each fail, without ever touching a value
that doesn't exist:

```csharp
var result = service
    .GetAccount(id)
    .Bind(account => service.ValidateWithdrawal(account, amount))
    .Bind(account => service.Withdraw(account, amount));
```

If any step in the chain fails, every step after it is skipped automatically —
the failure propagates straight through to the end, carrying its original error.

Turning the outcome into something usable — an HTTP response, a console message,
anything — by handling both branches explicitly:

```csharp
return result.Match(
    onSuccess: account => Results.Ok(account),
    onFailure: errors => Results.BadRequest(errors));
```

## Architecture

- **`Success(T value)` / `Failure(errors)`** — the only two ways a `Result<T>`
  can be constructed. The constructor itself is private, so it's impossible to
  create a `Result` that's simultaneously successful and carrying errors, or
  failed with no explanation.
- **`Bind`** — chains a second `Result`-returning operation onto the first.
  If the current result already failed, `next` is never called and the failure
  is passed straight through — the wrapped value is never touched on a failed
  path, which is what makes chaining safe.
- **`Match`** — the only way to extract a plain value out of a `Result<T>`.
  It requires a handler for *both* success and failure, so a failure can never
  be silently ignored the way it could be with a `bool` or `null`.
- **Implicit conversion from `T`** — lets a method return a plain value
  (`return account;`) instead of explicitly wrapping it (`return Result<Account>.Success(account);`)
  wherever a `Result<T>` is already the expected return type.

## Where it lives

`Result<T>` sits in `Application.Common`, not `Domain`. It's a convention for how
the *application layer* communicates outcomes to its callers — not a business
concept itself. Keeping it out of `Domain` means the domain entities stay usable
and understandable without depending on any particular way of reporting failure.

## Adding a new operation

Any method that can expectedly fail should return `Result<T>` instead of throwing
or returning `null`/`bool`:

```csharp
public Result<Account> ValidateDeposit(Account account, decimal amount)
{
    if (amount <= 0)
        return Result<Account>.Failure([
            new ErrorMessage("amount", "Deposit amount must be greater than zero.")
        ]);

    return account;
}
```

It slots straight into an existing `Bind` chain with no other changes needed.

## Design notes

- The constructor is private specifically to prevent an invalid state
  (`IsSuccess = true` with errors attached, or vice versa) from ever being
  constructible — this is enforced by the compiler, not by convention or a
  runtime check.
- `Value` stays private even though `IsSuccess` and `ErrorMessages` are exposed.
  The only way to safely reach the wrapped value is through `Bind` or `Match`,
  both of which guarantee `IsSuccess` has already been checked first.
- **Deliberately left out of this implementation:**
  - **Async support** (`Task<Result<T>>`, an async `Bind`) — only matters once
    real I/O (a database call, an external API) is involved. Nothing in this
    demo is I/O-bound, so it was left out rather than added speculatively.
  - **`Result.Combine(...)`** for merging several independent `Result`s into
    one — not needed by the current use case, but a natural extension the
    moment a scenario needs to report multiple unrelated failures at once
    (similar in spirit to how `Validator<T>` collects every rule violation).
  - **A non-generic `Result`** (no value, success/failure only) — built and
    tested earlier while designing this pattern, useful for operations with
    nothing meaningful to return (e.g. a delete). Left out of the final version
    since every operation here produces a value worth returning.