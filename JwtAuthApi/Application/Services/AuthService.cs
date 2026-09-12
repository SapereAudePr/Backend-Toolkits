using Application.Common;
using Application.Common.Interfaces;
using Application.DTOs;
using Application.Dummies;
using Application.Validation.Validate;
using Domain.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class AuthService(IApplicationDbContext dbContext, IPasswordHasher hasher, ITokenService tokenService)
    : IAuthService
{
    public async Task<Result<AuthResponseDto>> LoginAsync(LoginDto dto)
    {
        var validation = LoginValidation.ValidateLogin(dto);
        if (!validation.IsValid)
            return Result<AuthResponseDto>.Failure(
                validation.Errors.Select(x => new ErrorMessage(x.Field, x.Message)).ToList(),
                ResultStatus.ValidationFailure);

        var user = await dbContext.Users
            .AsNoTracking()
            .Where(u => string.Equals(u.Name, dto.Name))
            .Select(u => new
            {
                u.Id,
                u.Name,
                u.HashedPassword
            })
            .FirstOrDefaultAsync();


        if (user is null)
        {
            hasher.Verify(DummyPassword.Value, dto.Password);

            return Unauthorized();
        }

        if (!hasher.Verify(user.HashedPassword, dto.Password))
            return Unauthorized();

        var userDto = new UserDto { Id = user.Id, Name = user.Name };

        return Result<AuthResponseDto>.Success(await IssueTokensAsync(userDto));

        Result<AuthResponseDto> Unauthorized() =>
            Result<AuthResponseDto>.Failure(
                [
                    new ErrorMessage(
                        "Login", "Username or password is wrong")
                ],
                ResultStatus.Unauthorized);
    }

    //TODO:
    // Periodic deletion for too old revoked tokens
    public async Task<Result<AuthResponseDto>> RefreshAsync(RefreshRequestDto dto)
    {
        var validateToken = RefreshTokenValidation.ValidateRefreshToken(dto);
        if (!validateToken.IsValid)
            return Unauthorized();

        var refreshTokenValue = dto.RefreshToken.GetString()!;

        var hashedToken = tokenService.HashToken(refreshTokenValue);

        var storedToken = await dbContext.RefreshTokens.AsNoTracking().FirstOrDefaultAsync(x =>
            x.TokenHash == hashedToken);

        if (storedToken is null)
            return Unauthorized();

        if (storedToken.RevokedAt is not null)
        {
            await RevokeAllActiveUserTokensAsync(storedToken.UserId);

            return Unauthorized();
        }

        if (!storedToken.IsActive)
            return Unauthorized();

        var rowsAffected = await dbContext.RefreshTokens.Where(t =>
                t.Id == storedToken.Id && t.RevokedAt == null)
            .ExecuteUpdateAsync(setters => setters.SetProperty(
                t => t.RevokedAt, DateTimeOffset.UtcNow));

        if (rowsAffected == 0)
        {
            await RevokeAllActiveUserTokensAsync(storedToken.UserId);

            return Unauthorized();
        }

        var user = await dbContext.Users.AsNoTracking().Where(u =>
                u.Id == storedToken.UserId)
            .Select(u => new UserDto { Id = u.Id, Name = u.Name })
            .FirstOrDefaultAsync();

        if (user is null)
            return Unauthorized();

        return Result<AuthResponseDto>.Success(await IssueTokensAsync(user));

        Result<AuthResponseDto> Unauthorized() =>
            Result<AuthResponseDto>.Failure(
                [new ErrorMessage("RefreshToken", "Invalid or expired token")],
                ResultStatus.Unauthorized);
    }

    private async Task<AuthResponseDto> IssueTokensAsync(UserDto dto)
    {
        var accessToken = tokenService.GenerateAccessToken(dto);
        var rawRefreshToken = tokenService.GenerateRefreshToken();
        var hashedRefreshToken = tokenService.HashToken(rawRefreshToken);

        var refreshToken = new RefreshToken(
            hashedRefreshToken, dto.Id, expiresAt: DateTimeOffset.UtcNow.AddDays(7));

        await dbContext.RefreshTokens.AddAsync(refreshToken);
        await dbContext.SaveChangesAsync();

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = rawRefreshToken
        };
    }

    private async Task RevokeAllActiveUserTokensAsync(int userId)
    {
        List<RefreshToken> tokens = await dbContext.RefreshTokens.Where(t =>
                t.UserId == userId && t.RevokedAt == null)
            .ToListAsync();

        foreach (var t in tokens)
            t.Revoke();

        await dbContext.SaveChangesAsync();
    }
}