using Application.Common;
using Application.DTOs;
using Application.Mappings;
using Application.Validation.Validate;
using Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

// TODO: Write Validation and use it here

public class UserService(IApplicationDbContext dbContext) : IUserService
{
    public async Task<Result<List<UserDto>>> GetUsers()
    {
        var users = await dbContext.Users.ToListAsync();

        return Result<List<UserDto>>.Success(users.ToDto());
    }

    public async Task<Result<UserDto>> GetUser(int id)
    {
        var user = await dbContext.Users.FindAsync(id);
        if (user is null)
            return Result<UserDto>.Failure
            ([new ErrorMessage("id", "User could not found")],
                ResultStatus.NotFound);


        return Result<UserDto>.Success(user.ToDto());
    }

    public async Task<Result<UserDto>> CreateUser(CreateUserDto userDto)
    {
        var validation = UserValidation.ValidateUserCreation(userDto);
        if (!validation.IsValid)
        {
            var errors = validation.Errors.Select
                    (x => new ErrorMessage(x.Field, x.Message))
                .ToList();

            return Result<UserDto>.Failure(errors, ResultStatus.ValidationFailure);
        }

        var user = userDto.ToDomain();

        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync();

        return Result<UserDto>.Success(user.ToDto());
    }

    public async Task<Result<UserDto>> DeleteUser(int id)
    {
        var user = await dbContext.Users.FindAsync(id);
        if (user is null)
            return Result<UserDto>.Failure(
            [
                new ErrorMessage
                    ("Id", "User could not found")
            ], ResultStatus.NotFound);

        dbContext.Users.Remove(user);
        await dbContext.SaveChangesAsync();

        return Result<UserDto>.Success(user.ToDto(), ResultStatus.Deleted);
    }
}