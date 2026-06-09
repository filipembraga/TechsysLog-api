using TechsysLog.Application.DTOs.Requests;
using TechsysLog.Application.DTOs.Responses;

namespace TechsysLog.Application.Interfaces;

public interface IUserService
{
    Task<UserResponseDto> RegisterAsync(CreateUserDto dto);
    Task<string> LoginAsync(LoginDto dto);
}