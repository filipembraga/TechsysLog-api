using FluentAssertions;
using Microsoft.AspNetCore.Http;
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

        var loginResponse = new LoginResponseDto
        {
            Token = "valid.jwt.token",
            User = new UserBaseDto
            {
                Id = "6a29ccb85c6f09702e1853de",
                Name = "Zé das Couves",
                Email = "usuariodeTeste@techsyslog.com"
            }
        };

        _userServiceMock.Setup(s => s.LoginAsync(dto))
            .ReturnsAsync((loginResponse, "refresh-token-value", DateTime.UtcNow.AddDays(7)));

        _sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        // Act
        var result = await _sut.LoginAsync(dto);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
        .Which.StatusCode.Should().Be(200);
        _sut.Response.Headers["Set-Cookie"].ToString().Should().Contain("refreshToken=");
    }

    [Fact]
    public async Task RefreshAsync_WithValidCookie_Returns200WithNewToken()
    {
        // Arrange
        var loginResponse = new LoginResponseDto
        {
            Token = "new.jwt.token",
            User = new UserBaseDto { Id = "6a29ccb85c6f09702e1853de", Name = "Zé das Couves", Email = "user@techsyslog.com" }
        };

        _userServiceMock.Setup(s => s.RefreshAsync("valid-refresh-token")).ReturnsAsync(loginResponse);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Cookie"] = "refreshToken=valid-refresh-token";
        _sut.ControllerContext = new ControllerContext { HttpContext = httpContext };

        // Act
        var result = await _sut.RefreshAsync();

        // Assert
        result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task RefreshAsync_WithoutCookie_Returns401()
    {
        // Arrange
        _sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        // Act
        var result = await _sut.RefreshAsync();

        // Assert
        result.Should().BeOfType<UnauthorizedResult>();
        _userServiceMock.Verify(s => s.RefreshAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task LogoutAsync_WithCookie_DeletesCookieAndCallsService()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Cookie"] = "refreshToken=valid-refresh-token";
        _sut.ControllerContext = new ControllerContext { HttpContext = httpContext };

        // Act
        var result = await _sut.LogoutAsync();

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _userServiceMock.Verify(s => s.LogoutAsync("valid-refresh-token"), Times.Once);
    }
}