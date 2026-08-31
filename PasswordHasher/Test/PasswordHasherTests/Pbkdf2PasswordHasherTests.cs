using PasswordHasher.Security.Hashing;

namespace Test.PasswordHasherTests;

public class Pbkdf2PasswordHasherTests
{
    private readonly Pbkdf2PasswordHasher _hasher = new();

    [Fact]
    public void Hash_CalledTwiceWithSamePassword_ProducesDifferentOutput()
    {
        var hash1 = _hasher.Hash("MyPassword123");
        var hash2 = _hasher.Hash("MyPassword123");

        // Proves the salt is genuinely randomized per call, not fixed/reused.
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void Hash_ProducesThreePartFormat()
    {
        var hash = _hasher.Hash("MyPassword123");

        var parts = hash.Split('.');

        Assert.Equal(3, parts.Length);
        Assert.True(int.TryParse(parts[0], out _), "First part should be a parseable iteration count.");
    }

    [Fact]
    public void Verify_WithCorrectPassword_ReturnsTrue()
    {
        var hash = _hasher.Hash("MyPassword123");

        var result = _hasher.Verify(hash, "MyPassword123");

        Assert.True(result);
    }

    [Fact]
    public void Verify_WithIncorrectPassword_ReturnsFalse()
    {
        var hash = _hasher.Hash("MyPassword123");

        var result = _hasher.Verify(hash, "WrongGuess");

        Assert.False(result);
    }

    [Fact]
    public void Verify_IsCaseSensitive()
    {
        var hash = _hasher.Hash("MyPassword123");

        var result = _hasher.Verify(hash, "mypassword123");

        Assert.False(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-valid-hash")]
    [InlineData("100000.onlyonepart")]
    [InlineData("not-a-number.c2FsdA==.a2V5")]
    public void Verify_WithMalformedHash_ReturnsFalseInsteadOfThrowing(string malformedHash)
    {
        var result = _hasher.Verify(malformedHash, "AnyPassword");

        Assert.False(result);
    }

    [Fact]
    public void Verify_AgainstHashOfADifferentPassword_ReturnsFalse()
    {
        var hash = _hasher.Hash("FirstPassword");

        var result = _hasher.Verify(hash, "SecondPassword");

        Assert.False(result);
    }
}