using Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Application.Security;

public class PasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<object> _hasher = new();

    public string Hash(string password) => _hasher.HashPassword(null!, password);

    public bool Verify(string hashedPassword, string providedPassword)
    {
        var result = _hasher.VerifyHashedPassword(
            null!, hashedPassword, providedPassword);

        return result is
            PasswordVerificationResult.Success or
            PasswordVerificationResult.SuccessRehashNeeded;
    }
}