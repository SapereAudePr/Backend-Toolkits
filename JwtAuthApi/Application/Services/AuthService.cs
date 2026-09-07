using Application.Common;
using Application.Common.Interfaces;
using Application.DTOs;
using Application.Dummies;
using Application.Validation.Validate;
using Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class AuthService(IApplicationDbContext dbContext, IPasswordHasher hasher)
    : IAuthService
{
    public async Task<Result<UserDto>> Login(LoginDto dto)
    {
        var validation = LoginValidation.ValidateLogin(dto);
        if (!validation.IsValid)
            return Result<UserDto>.Failure(
                validation.Errors.Select(x => new ErrorMessage(x.Field, x.Message)).ToList(),
                ResultStatus.ValidationFailure);

        var result = await dbContext.Users
            .AsNoTracking()
            .Where(u => string.Equals(u.Name, dto.Name))
            .Select(u => new
            {
                u.Id,
                u.Name,
                u.HashedPassword
            })
            .FirstOrDefaultAsync();


        if (result is null)
        {
            hasher.Verify(
                DummyPassword.Value,
                dto.Password);

            return Result<UserDto>.Failure(
            [
                new ErrorMessage(
                    "Login",
                    "Username or password is wrong")
            ], ResultStatus.Unauthorized);
        }

        var verifyPassword = hasher.Verify(result.HashedPassword, dto.Password);

        return verifyPassword
            ? Result<UserDto>.Success(new UserDto
            {
                Id = result.Id,
                Name = result.Name
            })
            : Result<UserDto>.Failure([
                new ErrorMessage("Login", "Username or password is wrong")
            ], ResultStatus.Unauthorized);
    }
}