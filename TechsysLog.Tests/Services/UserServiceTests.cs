using FluentAssertions;
using Moq;
using TechsysLog.Application.DTOs.Requests;
using TechsysLog.Application.Interfaces;
using TechsysLog.Application.Services;
using TechsysLog.Domain.Common;
using TechsysLog.Domain.Interfaces;
using TechsysLog.Tests.Builders;

namespace TechsysLog.Tests.Services;

/// <summary>
/// Unit tests for UserService.
/// Repository and TokenService are mocked — only business logic is tested.
/// </summary>
public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepository;
    private readonly IUserService _sut;

    public UserServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _tokenServiceMock = new Mock<ITokenService>();
        _refreshTokenRepository = new Mock<IRefreshTokenRepository>();
        _sut = new UserService(_userRepositoryMock.Object, _tokenServiceMock.Object, _refreshTokenRepository.Object);
    }

    [Fact]
    public async Task RegisterAsync_WithValidData_ReturnsUserResponseDto()
    {
        // Arrange
        var dto = new CreateUserDto
        {
            Name = "Filipe Braga",
            Email = "filipe@techsyslog.com",
            Password = "Test@1234"
        };

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(dto.Email))
            .ReturnsAsync((Domain.Entities.User?)null);


        _userRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<Domain.Entities.User>()))
            .Callback<Domain.Entities.User>(u => u.Id = "6a29ccb85c6f09702e1853de")
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.RegisterAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be(dto.Email);
        result.Name.Should().Be(dto.Name);
        result.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task RegisterAsync_WithDuplicateEmail_ThrowsInvalidOperationException()
    {
        // Arrange
        var existingUser = new UserBuilder()
            .WithEmail("existing@techsyslog.com")
            .Build();

        var dto = new CreateUserDto
        {
            Name = "Another User",
            Email = "existing@techsyslog.com",
            Password = "Test@1234"
        };

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(dto.Email))
            .ReturnsAsync(existingUser);

        // Act
        var act = async () => await _sut.RegisterAsync(dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Email already registered.");
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsToken()
    {
        // Arrange
        var password = "Test@1234";
        var user = new UserBuilder()
            .WithEmail("filipe@techsyslog.com")
            .WithPasswordHash(BCrypt.Net.BCrypt.HashPassword(password))
            .Build();

        var dto = new LoginDto
        {
            Email = user.Email,
            Password = password
        };

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(dto.Email))
            .ReturnsAsync(user);

        _tokenServiceMock
            .Setup(j => j.GenerateAccessToken(user.Id, user.Email))
            .Returns("valid.jwt.token");

        _tokenServiceMock
            .Setup(j => j.GenerateRefreshToken())
            .Returns("valid-refresh-token");


        // Act
        var result = await _sut.LoginAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Response.Token.Should().NotBeEmpty();
        _refreshTokenRepository.Verify(r => r.CreateAsync(It.IsAny<Domain.Entities.RefreshToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var user = new UserBuilder()
            .WithPasswordHash(BCrypt.Net.BCrypt.HashPassword("CorrectPassword"))
            .Build();

        var dto = new LoginDto
        {
            Email = user.Email,
            Password = "WrongPassword"
        };

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(dto.Email))
            .ReturnsAsync(user);

        // Act
        var act = async () => await _sut.LoginAsync(dto);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid email or password.");
    }

    [Fact]
    public async Task LoginAsync_WithNonExistentEmail_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((Domain.Entities.User?)null);

        var dto = new LoginDto
        {
            Email = "nonexistent@techsyslog.com",
            Password = "Test@1234"
        };

        // Act
        var act = async () => await _sut.LoginAsync(dto);

        // Assert — same error as wrong password to prevent user enumeration
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid email or password.");
    }

    [Fact]
    public async Task RefreshAsync_WithValidToken_ReturnsNewAccessToken()
    {
        // Arrange
        var user = new UserBuilder().Build();
        var storedToken = new Domain.Entities.RefreshToken
        {
            UserId = user.Id,
            TokenHash = TokenHasher.Hash("valid-refresh-token"),
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };

        _refreshTokenRepository.Setup(r => r.GetByHashAsync(It.IsAny<string>())).ReturnsAsync(storedToken);
        _userRepositoryMock.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);
        _tokenServiceMock.Setup(j => j.GenerateAccessToken(user.Id, user.Email)).Returns("new.jwt.token");

        // Act
        var result = await _sut.RefreshAsync("valid-refresh-token");

        // Assert
        result.Token.Should().Be("new.jwt.token");
    }

    [Fact]
    public async Task RefreshAsync_WithUnknownToken_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        _refreshTokenRepository.Setup(r => r.GetByHashAsync(It.IsAny<string>()))
            .ReturnsAsync((Domain.Entities.RefreshToken?)null);

        // Act
        var act = async () => await _sut.RefreshAsync("unknown-token");

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid or expired refresh token.");
    }

    [Fact]
    public async Task RefreshAsync_WithExpiredToken_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var expiredToken = new Domain.Entities.RefreshToken
        {
            UserId = "6a29ccb85c6f09702e1853de",
            TokenHash = TokenHasher.Hash("expired-token"),
            ExpiresAt = DateTime.UtcNow.AddDays(-1)
        };

        _refreshTokenRepository.Setup(r => r.GetByHashAsync(It.IsAny<string>())).ReturnsAsync(expiredToken);

        // Act
        var act = async () => await _sut.RefreshAsync("expired-token");

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid or expired refresh token.");
    }

    [Fact]
    public async Task RefreshAsync_WhenUserNoLongerExists_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var storedToken = new Domain.Entities.RefreshToken
        {
            UserId = "deleted-user-id",
            TokenHash = TokenHasher.Hash("valid-refresh-token"),
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };

        _refreshTokenRepository.Setup(r => r.GetByHashAsync(It.IsAny<string>())).ReturnsAsync(storedToken);
        _userRepositoryMock.Setup(r => r.GetByIdAsync("deleted-user-id")).ReturnsAsync((Domain.Entities.User?)null);

        // Act
        var act = async () => await _sut.RefreshAsync("valid-refresh-token");

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid or expired refresh token.");
    }

    [Fact]
    public async Task LogoutAsync_DeletesRefreshTokenByHash()
    {
        // Arrange
        var refreshToken = "valid-refresh-token";
        var expectedHash = TokenHasher.Hash(refreshToken);

        // Act
        await _sut.LogoutAsync(refreshToken);

        // Assert
        _refreshTokenRepository.Verify(r => r.DeleteByHashAsync(expectedHash), Times.Once);
    }
}