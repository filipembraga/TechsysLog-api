using TechsysLog.Application.DTOs.Requests;
using TechsysLog.Application.DTOs.Responses;
using TechsysLog.Application.Interfaces;
using TechsysLog.Domain.Common;
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
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    private const int RefreshTokenExpirationDays = 7;

    public UserService(
        IUserRepository userRepository,
        ITokenService tokenService,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _refreshTokenRepository = refreshTokenRepository;
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

    public async Task<(LoginResponseDto Response, string RefreshToken, DateTime RefreshTokenExpiresAt)> LoginAsync(LoginDto dto)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email);

        // If not found, we fall through to the same error as wrong password
        if (user is null || !BC.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        var token = _tokenService.GenerateAccessToken(user.Id, user.Email);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddDays(RefreshTokenExpirationDays);

        await _refreshTokenRepository.CreateAsync(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = TokenHasher.Hash(refreshToken),
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow
        });

        var response = new LoginResponseDto
        {
            Token = token,
            User = MapToResponse(user)
        };

        return (response, refreshToken, expiresAt);
    }

    public async Task<LoginResponseDto> RefreshAsync(string refreshToken)
    {
        var tokenHash = TokenHasher.Hash(refreshToken);
        var storedToken = await _refreshTokenRepository.GetByHashAsync(tokenHash);

        if (storedToken is null || storedToken.ExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");

        var user = await _userRepository.GetByIdAsync(storedToken.UserId);

        if (user is null)
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");

        var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Email);

        return new LoginResponseDto
        {
            Token = accessToken,
            User = MapToResponse(user)
        };
    }

    public async Task LogoutAsync(string refreshToken)
    {
        var tokenHash = TokenHasher.Hash(refreshToken);
        await _refreshTokenRepository.DeleteByHashAsync(tokenHash);
    }

    private static UserResponseDto MapToResponse(User user) => new()
    {
        Id = user.Id,
        Name = user.Name,
        Email = user.Email,
        CreatedAt = user.CreatedAt
    };
}