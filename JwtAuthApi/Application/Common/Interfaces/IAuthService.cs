using Application.DTOs;
using Domain.Common;

namespace Application.Common.Interfaces;

public interface IAuthService
{
    Task<Result<UserDto>> Login(LoginDto dto);
}