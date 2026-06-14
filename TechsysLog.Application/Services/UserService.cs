using TechsysLog.Application.DTOs.Requests;
using TechsysLog.Application.DTOs.Responses;
using TechsysLog.Application.Interfaces;
using TechsysLog.Domain.Entities;
using TechsysLog.Domain.Interfaces;
using BC = BCrypt.Net.BCrypt;

namespace TechsysLog.Application.Services;

/// <summary>
/// Handles user registration and authentication.
///
/// Design decision: password hashing is delegated to BCrypt.Net.
/// The plain-text password is never stored or logged at any point 
/// only the hash reaches the repository.
///
/// Design decision: login returns a JWT token string directly.
/// </summary>
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;

    public UserService(
        IUserRepository userRepository,
        IJwtService jwtService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
    }

    public async Task<UserResponseDto> RegisterAsync(CreateUserDto dto)
    {
        var emailAlreadyExists = await _userRepository.GetByEmailAsync(dto.Email);

        if (emailAlreadyExists is not null)
            throw new InvalidOperationException("Email already registered.");

        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            PasswordHash = BC.HashPassword(dto.Password), // Note that Password from the DTO is never stored as-is. We hash it before saving to the database.
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.CreateAsync(user);

        return MapToResponse(user); // MapToResponse produces a safe DTO with only public fields.
    }

    public async Task<LoginResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email);

        // If not found, we fall through to the same error as wrong password
        if (user is null || !BC.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        var token = _jwtService.GenerateToken(user.Id, user.Email);
        return new LoginResponseDto
        {
            Token = token,
            User = MapToResponse(user)
        };
    }

    private static UserResponseDto MapToResponse(User user) => new()
    {
        Id = user.Id,
        Name = user.Name,
        Email = user.Email,
        CreatedAt = user.CreatedAt
    };
}