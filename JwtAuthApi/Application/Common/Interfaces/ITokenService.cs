using Application.DTOs;

namespace Application.Common.Interfaces;

public interface ITokenService
{
    string GenerateToken(UserDto dto);
}