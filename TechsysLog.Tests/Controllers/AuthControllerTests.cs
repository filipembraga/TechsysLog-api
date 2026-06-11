using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TechsysLog.API.Controllers;
using TechsysLog.Application.DTOs.Requests;
using TechsysLog.Application.DTOs.Responses;
using TechsysLog.Application.Interfaces;

namespace TechsysLog.Tests.Controllers;

/// <summary>
/// Unit tests for AuthController.
/// IUserService is mocked — HTTP pipeline is not involved.
/// </summary>
public class AuthControllerTests
{
    private readonly Mock<IUserService> _userServiceMock;
    private readonly AuthController _sut;

    public AuthControllerTests()
    {
        _userServiceMock = new Mock<IUserService>();
        _sut = new AuthController(_userServiceMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_WithValidData_Returns201WithUser()
    {
        // Arrange
        var dto = new CreateUserDto
        {
            Name = "Filipe Braga",
            Email = "filipe@techsyslog.com",
            Password = "Test@1234"
        };

        var responseDto = new UserResponseDto
        {
            Id = "6a29ccb85c6f09702e1853de",
            Name = dto.Name,
            Email = dto.Email,
            CreatedAt = DateTime.UtcNow
        };

        _userServiceMock.Setup(s => s.RegisterAsync(dto)).ReturnsAsync(responseDto);

        // Act
        var result = await _sut.RegisterAsync(dto);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
        createdResult.Value.Should().BeEquivalentTo(responseDto);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_Returns200WithToken()
    {
        // Arrange
        var dto = new LoginDto
        {
            Email = "filipe@techsyslog.com",
            Password = "Test@1234"
        };

        _userServiceMock.Setup(s => s.LoginAsync(dto))
            .ReturnsAsync("valid.jwt.token");

        // Act
        var result = await _sut.LoginAsync(dto);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(new { token = "valid.jwt.token" });
    }

    [Fact]
    public async Task RegisterAsync_WhenServiceThrows_ExceptionPropagates()
    {
        // Arrange — middleware handles this in production
        var dto = new CreateUserDto
        {
            Name = "Test",
            Email = "duplicate@techsyslog.com",
            Password = "Test@1234"
        };

        _userServiceMock.Setup(s => s.RegisterAsync(dto))
            .ThrowsAsync(new InvalidOperationException("Email already registered."));

        // Act
        var act = async () => await _sut.RegisterAsync(dto);

        // Assert — exception propagates to ExceptionHandlingMiddleware
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Email already registered.");
    }
}