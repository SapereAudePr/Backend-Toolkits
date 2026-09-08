using Application.DTOs;

namespace Application.Common.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(UserDto dto);
    string GenerateRefreshToken();
    string HashToken(string rawToken);
}