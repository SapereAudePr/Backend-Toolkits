using Application.Common;
using Application.Common.Interfaces;
using Application.DTOs;
using Application.Mappings;
using Application.Validation.Validate;
using Domain.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class UserService(
    IApplicationDbContext dbContext,
    IPasswordHasher hasher) :
    IUserService
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

        var hashedPassword = hasher.Hash(userDto.Password);

        var user = new User(userDto.Name, hashedPassword, userDto.CreatedBy);

        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync();

        return Result<UserDto>.Success(user.ToDto());
    }

    public async Task<Result<UserDto>> UpdateUser(int id, UpdateUserDto userDto)
    {
        var user = await dbContext.Users.FindAsync(id);
        if (user is null)
            return Result<UserDto>.Failure(
                [
                    new ErrorMessage
                        ("id", $"User could not found with Id: {id}")
                ],
                ResultStatus.NotFound);

        var validation = UserValidation.ValidateUserUpdate(userDto);
        if (!validation.IsValid)
            return Result<UserDto>.Failure(validation.Errors.Select(x =>
                new ErrorMessage(x.Field, x.Message
                )).ToList(), ResultStatus.ValidationFailure);

        user.ChangeName(userDto.Name);
        user.ChangePassword(hasher.Hash(userDto.Password));

        await dbContext.SaveChangesAsync();

        return Result<UserDto>.Success(user.ToDto());
    }

    public async Task<Result<UserDto>> PatchUser(int id, PatchUserDto userDto)
    {
        var userToUpdate = await dbContext.Users.FindAsync(id);
        if (userToUpdate is null)
            return Result<UserDto>.Failure(
                [
                    new ErrorMessage
                        ("id", $"User could not found with Id: {id}")
                ],
                ResultStatus.NotFound);

        var validation = UserValidation.ValidateUserPatch(userDto);
        if (!validation.IsValid)
            return Result<UserDto>.Failure(
                validation.Errors.Select
                    (x => new ErrorMessage(x.Field, x.Message)).ToList(),
                ResultStatus.ValidationFailure);

        if (userDto.Name is not null)
            userToUpdate.ChangeName(userDto.Name);
        if (userDto.Password is not null)
            userToUpdate.ChangePassword(hasher.Hash(userDto.Password));

        await dbContext.SaveChangesAsync();

        return Result<UserDto>.Success(userToUpdate.ToDto());
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