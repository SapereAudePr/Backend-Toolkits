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
    public async Task<Result<PagedResult<UserDto>>> GetUsers(UserQueryParameters parameters)
    {
        var query = dbContext.Users.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
        {
            var term = parameters.SearchTerm.Trim();
            query = query.Where(u =>
                EF.Functions.Like(u.Name, $"%{term}%"));
        }

        var normalizedSortDescending = parameters.NormalizedSortDescending;

        query = parameters.SortBy?.Trim().ToLower() switch
        {
            "name" => normalizedSortDescending
                ? query.OrderByDescending(u => u.Name)
                : query.OrderBy(u => u.Name),
            _ => normalizedSortDescending
                ? query.OrderByDescending(u => u.Id)
                : query.OrderBy(u => u.Id)
        };

        var totalCount = await query.CountAsync();

        var page = parameters.NormalizedPage;
        var pageSize = parameters.NormalizedPageSize;

        var users = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        var result = new PagedResult<UserDto>
        {
            Items = users.ToDto(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };

        return Result<PagedResult<UserDto>>.Success(result);
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
        user.SetHashedPassword(hasher.Hash(userDto.Password));

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
            userToUpdate.SetHashedPassword(hasher.Hash(userDto.Password));

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