namespace Domain.Entities;

public class RefreshToken
{
    public int Id { get; }
    public string TokenHash { get; private set; } = null!;
    public int UserId { get; private set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }

    private RefreshToken()
    {
    }

    public RefreshToken(string tokenHash, int userId, DateTimeOffset expiresAt)
    {
        TokenHash = tokenHash;
        UserId = userId;
        ExpiresAt = expiresAt;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public bool IsActive => RevokedAt is null && ExpiresAt > DateTimeOffset.UtcNow;

    public void Revoke() => RevokedAt = DateTimeOffset.UtcNow;
}