using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TechsysLog.API.Controllers;
using TechsysLog.Application.DTOs.Responses;
using TechsysLog.Application.Interfaces;

namespace TechsysLog.Tests.Controllers;

/// <summary>
/// Unit tests for NotificationController.
/// Exception propagation tests verify that ExceptionHandlingMiddleware.
/// </summary>
public class NotificationsControllerTests
{
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly NotificationsController _sut;
    private const string UserId = "6a29ccb85c6f09702e1853de";

    public NotificationsControllerTests()
    {
        _notificationServiceMock = new Mock<INotificationService>();
        _sut = new NotificationsController(_notificationServiceMock.Object);

        // Simulate authenticated user
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, UserId),
            new(ClaimTypes.Email, "filipe@techsyslog.com")
        };

        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"))
            }
        };
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllNotifications()
    {
        // Arrange
        var notifications = new List<NotificationResponseDto>
        {
            new() { Id = "id1", Message = "New order ORD-00001 registered.", IsRead = false },
            new() { Id = "id2", Message = "Order ORD-00001 has been delivered.", IsRead = true }
        };

        _notificationServiceMock
            .Setup(s => s.GetAllAsync())
            .ReturnsAsync(notifications);

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(notifications);
    }

    [Fact]
    public async Task GetUnreadAsync_ReturnsOnlyUnreadNotifications()
    {
        // Arrange
        var unread = new List<NotificationResponseDto>
        {
            new() { Id = "id1", Message = "New order ORD-00002 registered.", IsRead = false },
            new() { Id = "id2", Message = "New order ORD-00003 registered.", IsRead = false }
        };

        _notificationServiceMock
            .Setup(s => s.GetUnreadAsync())
            .ReturnsAsync(unread);

        // Act
        var result = await _sut.GetUnreadAsync();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(unread);
    }

    [Fact]
    public async Task MarkAsReadAsync_WithValidId_Returns204()
    {
        // Arrange
        var notificationId = "6a2a3513034b3271f27a233f";

        _notificationServiceMock
            .Setup(s => s.MarkAsReadAsync(notificationId, UserId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.MarkAsReadAsync(notificationId);

        // Assert
        result.Should().BeOfType<NoContentResult>()
            .Which.StatusCode.Should().Be(204);
    }

    [Fact]
    public async Task MarkAsReadAsync_PassesAuthenticatedUserIdToService()
    {
        // Arrange
        var notificationId = "6a2a3513034b3271f27a233f";

        _notificationServiceMock
            .Setup(s => s.MarkAsReadAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.MarkAsReadAsync(notificationId);

        // Assert — UserId from JWT claims was passed, not from request body
        _notificationServiceMock.Verify(
            s => s.MarkAsReadAsync(notificationId, UserId),
            Times.Once);
    }

    [Fact]
    public async Task MarkAsReadAsync_WithMissingUserClaim_ThrowsUnauthorizedAccessException()
    {
        // Arrange — controller with no claims simulates broken token
        var controllerWithNoClaims = new NotificationsController(_notificationServiceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity())
                }
            }
        };

        // Act
        var act = async () => await controllerWithNoClaims.MarkAsReadAsync("anyId");

        // Assert — ExceptionHandlingMiddleware maps this to 401
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("User identity not found in token.");
    }
}