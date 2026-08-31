# Password Hashing

A PBKDF2-based password hasher built directly from raw cryptography
primitives (`Rfc2898DeriveBytes`, `RandomNumberGenerator`), to understand
exactly what ASP.NET Core Identity's `PasswordHasher<TUser>` does internally
before relying on it — or an equivalent library — in a real project.

## The problem this solves

Passwords must never be stored as plaintext or encrypted (reversible) —
if the database leaks, either approach lets an attacker recover real
passwords. Hashing is one-way: there is no "unhash" operation. Verifying a
password never involves reversing the stored value — it involves re-running
the same computation on the new attempt and comparing the results.

Two further problems a naive hash (e.g. plain SHA-256) doesn't solve:

- **It's too fast.** General-purpose hash functions are designed to hash
  gigabytes per second, which lets an attacker with a stolen database try
  billions of password guesses per second. Password hashing must be
  deliberately slow.
- **Identical passwords produce identical hashes.** Without a salt, two
  users with the same password have the same hash, and attackers can
  precompute lookup tables (rainbow tables) of common passwords in advance.

## Usage

```csharp
var hasher = new Pbkdf2PasswordHasher();

// Registration
var stored = hasher.Hash("MyPassword123");
// -> "100000.qYh3s7...==.k9Lp2v...=="  (store this string in the database)

// Login attempt
var isValid = hasher.Verify(stored, "MyPassword123"); // true
var isValid2 = hasher.Verify(stored, "WrongGuess");   // false
```

## How it actually works

**Hashing (registration):**
1. Generate a random 128-bit salt using `RandomNumberGenerator` — never
   `System.Random`, which is seeded predictably and would make the salt
   guessable, defeating its entire purpose.
2. Run PBKDF2 with that salt, for a fixed number of iterations (100,000 in
   this implementation), producing a 256-bit derived key.
3. Pack the iteration count, salt, and key together into one string
   (`"{iterations}.{salt}.{key}"`, salt/key Base64-encoded) and store that
   single string. The salt isn't secret — it's stored openly alongside the
   hash, and its purpose isn't concealment, it's guaranteeing every password
   produces a unique computation.

**Verifying (login):**
1. Split the stored string back into iteration count, salt, and stored key.
2. Re-run the *exact same* PBKDF2 computation — same salt, same iteration
   count — but on the *newly typed* password.
3. Compare the newly computed key against the stored key, using
   `CryptographicOperations.FixedTimeEquals` rather than a normal `==` or
   `SequenceEqual`. A normal comparison exits as soon as it finds the first
   mismatched byte, meaning how long the comparison takes leaks information
   about how many leading bytes were already correct — an attacker
   measuring response times could exploit that to guess the key one byte at
   a time. A fixed-time comparison always takes the same amount of time
   regardless of where a mismatch occurs.

The identical password produces a *different* stored string every time
`Hash` is called (because the salt is randomized per call) — but `Verify`
still correctly returns `true` for the right password and `false` for the
wrong one, every time, because it re-derives using the salt that's actually
stored, not by comparing hash strings for equality.

## Comparison with ASP.NET Core Identity's `PasswordHasher<TUser>`

Conceptually identical — Identity's default implementation also uses PBKDF2
with HMAC-SHA256, a random salt, and a packed format containing everything
needed to verify later. The differences are mostly cosmetic:

| | This implementation | `PasswordHasher<TUser>` |
|---|---|---|
| Format | `iterations.salt.key`, dot-separated, Base64 parts | A single Base64 string with a binary-packed header |
| Iteration count | Fixed at 100,000 | Configurable via `PasswordHasherOptions`, defaults to 100,000 (current .NET versions) |
| Rehashing on outdated params | Not implemented | Signals `SuccessRehashNeeded` so callers can transparently upgrade old hashes |

## Design notes

- **Why build this instead of just using `PasswordHasher<TUser>`.** Purely
  educational — this class exists to make every step of the process
  visible and inspectable, the same reason `IExceptionHandler` was built by
  hand before adopting the framework version. For any real project, prefer
  a maintained implementation.
- **This implementation is not recommended for production use as-is.** It
  hasn't been reviewed for security issues, doesn't support upgrading the
  iteration count for old hashes without a migration path, and reinvents
  something ASP.NET Core Identity and other maintained libraries already
  solve correctly. It's a learning artifact, documented as such.
- **Deliberately left out:** Argon2/bcrypt support (PBKDF2 is the simplest
  to build from BCL primitives alone — Argon2 requires either a third-party
  library or significantly more custom implementation), and automatic
  rehashing when the iteration count is out of date.