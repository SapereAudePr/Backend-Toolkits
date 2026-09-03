using Application.DTOs;
using Domain.Common;

namespace Application.Common.Interfaces;

public interface IUserService
{
    public Task<Result<PagedResult<UserDto>>> GetUsers(UserQueryParameters parameters);
    public Task<Result<UserDto>> GetUser(int id);
    public Task<Result<UserDto>> CreateUser(CreateUserDto userDto);
    public Task<Result<UserDto>> UpdateUser(int id, UpdateUserDto userDto);
    public Task<Result<UserDto>> PatchUser(int id, PatchUserDto userDto);
    public Task<Result<UserDto>> DeleteUser(int id);
}