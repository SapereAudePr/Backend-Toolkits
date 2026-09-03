using Application.DTOs;
using Domain.Common;

namespace Application.Common.Interfaces;

public interface IAuthService
{
    Task<Result<bool>> Login(LoginDto dto);
}