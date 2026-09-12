using Application.DTOs;
using Domain.Common;

namespace Application.Common.Interfaces;

public interface IAuthService
{
    Task<Result<AuthResponseDto>> LoginAsync(LoginDto dto);
    Task<Result<AuthResponseDto>> RefreshAsync(RefreshRequestDto dto);
}