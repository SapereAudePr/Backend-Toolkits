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
    public async Task<Result<bool>> Login(LoginDto dto)
    {
        var validation = LoginValidation.ValidateLogin(dto);
        if (!validation.IsValid)
            return Result<bool>.Failure(
                validation.Errors.Select(
                    x => new ErrorMessage(x.Field, x.Message)).ToList(),
                ResultStatus.ValidationFailure);

        var result = await dbContext.Users.FirstOrDefaultAsync(u =>
            string.Equals(u.Name, dto.Name));
        if (result is null)
        {
            hasher.Verify(
                DummyPassword.Value,
                dto.Password);
            
            return Result<bool>.Failure(
            [
                new ErrorMessage(
                    "Login",
                    "Username or password is wrong")
            ], ResultStatus.Unauthorized);
        }
        
        var verifyPassword = hasher.Verify(result.HashedPassword, dto.Password);

        return verifyPassword
            ? Result<bool>.Success(true)
            : Result<bool>.Failure([
                new ErrorMessage("Login", "Username or password is wrong")
            ], ResultStatus.Unauthorized);
    }
}