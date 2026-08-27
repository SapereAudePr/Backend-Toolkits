using Application.DTOs;
using Domain.Common;

namespace Application.Services;

public interface IUserService
{
    public Task<Result<List<UserDto>>> GetUsers();
    public Task<Result<UserDto>> GetUser(int id);
    public Task<Result<UserDto>> CreateUser(CreateUserDto userDto);
    public Task<Result<UserDto>> UpdateUser(int id, UpdateUserDto userDto);
    public Task<Result<UserDto>> PatchUser(int id, PatchUserDto userDto);
    public Task<Result<UserDto>> DeleteUser(int id);
}