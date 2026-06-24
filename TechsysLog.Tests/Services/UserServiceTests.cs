using FluentAssertions;
using Moq;
using TechsysLog.Application.DTOs.Requests;
using TechsysLog.Application.Interfaces;
using TechsysLog.Application.Services;
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
    private readonly IUserService _sut;

    public UserServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _tokenServiceMock = new Mock<ITokenService>();
        _sut = new UserService(_userRepositoryMock.Object, _tokenServiceMock.Object);
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
            .Setup(j => j.GenerateToken(user.Id, user.Email))
            .Returns("valid.jwt.token");

        // Act
        var result = await _sut.LoginAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().NotBeEmpty();
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
}