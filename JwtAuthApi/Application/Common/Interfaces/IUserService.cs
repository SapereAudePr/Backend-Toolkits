using Application.DTOs;
using Domain.Common;

namespace Application.Common.Interfaces;

public interface IUserService
{
    public Task<Result<PagedResult<UserDto>>> GetUsersAsync(UserQueryParameters parameters);
    public Task<Result<UserDto>> GetUserAsync(int id);
    public Task<Result<UserDto>> CreateUserAsync(CreateUserDto userDto);
    public Task<Result<UserDto>> UpdateUserAsync(int id, UpdateUserDto userDto);
    public Task<Result<UserDto>> PatchUserAsync(int id, PatchUserDto userDto);
    public Task<Result<UserDto>> DeleteUserAsync(int id);
}