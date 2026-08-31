using System.Security.Cryptography;

namespace PasswordHasher.Security.Hashing;

/// <summary>
/// A password hasher built directly on PBKDF2 primitives, rather than
/// wrapping ASP.NET Core Identity's <c>PasswordHasher&lt;TUser&gt;</c>.
/// Built to understand exactly what that class does internally.
/// </summary>
/// <remarks>
/// For real production code, prefer a maintained, audited implementation
/// (ASP.NET Core Identity's <c>PasswordHasher&lt;TUser&gt;</c>, or a library
/// implementing Argon2/bcrypt) rather than this class. Hand-rolling
/// cryptographic code is valuable for learning exactly what's happening,
/// but maintained libraries get security review and patches this class
/// never will.
/// </remarks>
public class Pbkdf2PasswordHasher
{
    private const int SaltSizeBytes = 16;     // 128-bit salt
    private const int KeySizeBytes = 32;      // 256-bit derived key
    private const int Iterations = 100_000;   // work factor — the deliberate "slowness"
    private static readonly HashAlgorithmName Prf = HashAlgorithmName.SHA256;

    /// <summary>
    /// Hashes a plaintext password using PBKDF2 with a freshly generated,
    /// cryptographically random salt, and packs the iteration count, salt,
    /// and derived key into a single storable string.
    /// </summary>
    /// <param name="password">The plaintext password to hash.</param>
    /// <returns>
    /// A string of the form <c>"{iterations}.{salt}.{key}"</c>, with the
    /// salt and key Base64-encoded, suitable for storing directly in a
    /// database column.
    /// </returns>
    public string Hash(string password)
    {
        // RandomNumberGenerator, NOT System.Random — System.Random is
        // predictable (seeded from time), which would make the salt
        // guessable and defeat its entire purpose.
        var salt = RandomNumberGenerator.GetBytes(SaltSizeBytes);

        var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Prf, KeySizeBytes);

        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
    }

    /// <summary>
    /// Verifies a plaintext password attempt against a previously hashed
    /// value, by re-deriving a key with the same salt and iteration count
    /// that were used originally, then comparing the results.
    /// </summary>
    /// <param name="hashedPassword">The stored hash, as produced by <see cref="Hash"/>.</param>
    /// <param name="providedPassword">The plaintext password attempt to check.</param>
    /// <returns>
    /// <see langword="true"/> if the provided password matches; otherwise
    /// <see langword="false"/>, including when <paramref name="hashedPassword"/>
    /// is malformed.
    /// </returns>
    public bool Verify(string hashedPassword, string providedPassword)
    {
        var parts = hashedPassword.Split('.');
        if (parts.Length != 3)
            return false;

        if (!int.TryParse(parts[0], out var iterations))
            return false;

        byte[] salt, storedKey;
        try
        {
            salt = Convert.FromBase64String(parts[1]);
            storedKey = Convert.FromBase64String(parts[2]);
        }
        catch (FormatException)
        {
            return false;
        }

        var computedKey = Rfc2898DeriveBytes.Pbkdf2(providedPassword, salt, iterations, Prf, storedKey.Length);

        // CryptographicOperations.FixedTimeEquals, NOT computedKey == storedKey
        // or Enumerable.SequenceEqual. A normal comparison returns as soon as
        // it finds the first mismatched byte, so the time it takes leaks how
        // many leading bytes were correct — an attacker measuring response
        // times could exploit that to guess the key one byte at a time.
        // FixedTimeEquals always takes the same amount of time regardless of
        // where (or whether) a mismatch occurs.
        return CryptographicOperations.FixedTimeEquals(computedKey, storedKey);
    }
}