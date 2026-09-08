using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Application.Common.Interfaces;
using Application.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Application.Security;

public class TokenService(IConfiguration conf) : ITokenService
{
    public string GenerateAccessToken(UserDto dto)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, dto.Id.ToString()),
            new(JwtRegisteredClaimNames.Name, dto.Name)
        };

        var signingKey = conf["Jwt:SigningKey"] ?? throw new
            InvalidOperationException("Jwt signing key is not configured");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: conf["Jwt:Issuer"],
            audience: conf["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    public string HashToken(string rawToken) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));
}