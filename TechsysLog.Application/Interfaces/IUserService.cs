using TechsysLog.Application.DTOs.Requests;
using TechsysLog.Application.DTOs.Responses;

namespace TechsysLog.Application.Interfaces;

public interface IUserService
{
    Task<UserResponseDto> RegisterAsync(CreateUserDto dto);
    Task<(LoginResponseDto Response, string RefreshToken, DateTime RefreshTokenExpiresAt)> LoginAsync(LoginDto dto);
    Task<LoginResponseDto> RefreshAsync(string refreshToken);
    Task LogoutAsync(string refreshToken);
}