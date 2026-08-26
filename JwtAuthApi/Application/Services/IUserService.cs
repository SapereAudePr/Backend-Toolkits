using Application.DTOs;
using Domain.Common;

namespace Application.Services;

public interface IUserService
{
    public Task<Result<List<UserDto>>> GetUsers();
    public Task<Result<UserDto>> GetUser(int id);
    public Task<Result<UserDto>> CreateUser(CreateUserDto userDto);
}