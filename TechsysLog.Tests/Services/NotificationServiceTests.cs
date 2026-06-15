using FluentAssertions;
using Moq;
using TechsysLog.Application.Interfaces;
using TechsysLog.Application.Services;
using TechsysLog.Domain.Enums;
using TechsysLog.Domain.Interfaces;
using TechsysLog.Tests.Builders;

namespace TechsysLog.Tests.Services;

/// <summary>
/// Unit tests for NotificationService.
/// </summary>
public class NotificationServiceTests
{
    private readonly Mock<INotificationRepository> _notificationRepositoryMock;
    private readonly Mock<INotificationDispatcher> _notificationDispatcherMock;
    private readonly INotificationService _sut;

    public NotificationServiceTests()
    {
        _notificationRepositoryMock = new Mock<INotificationRepository>();
        _notificationDispatcherMock = new Mock<INotificationDispatcher>();

        _sut = new NotificationService(
            _notificationRepositoryMock.Object,
            _notificationDispatcherMock.Object);
    }

    [Fact]
    public async Task SendAsync_PersistsNotificationAndDispatches()
    {
        // Arrange
        _notificationRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<Domain.Entities.Notification>()))
            .Returns(Task.CompletedTask);

        _notificationDispatcherMock
            .Setup(d => d.SendNotificationAsync(It.IsAny<Domain.Entities.Notification>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.SendAsync("New order ORD-00001 registered.", "orderId123", NotificationType.OrderRegistered);

        // Assert — both persist and dispatch were called exactly once
        _notificationRepositoryMock.Verify(
            r => r.CreateAsync(It.Is<Domain.Entities.Notification>(n =>
                n.Message == "New order ORD-00001 registered." &&
                n.OrderId == "orderId123" &&
                n.Type == NotificationType.OrderRegistered &&
                n.IsRead == false)),
            Times.Once);

        _notificationDispatcherMock.Verify(
            d => d.SendNotificationAsync(It.IsAny<Domain.Entities.Notification>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllNotificationsMapped()
    {
        // Arrange
        var notifications = new List<Domain.Entities.Notification>
        {
            new NotificationBuilder().Build(),
            new NotificationBuilder().WithId("6a2a3513034b3271f27a2340").AsRead().Build()
        };

        _notificationRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(notifications);

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result[0].IsRead.Should().BeFalse();
        result[1].IsRead.Should().BeTrue();
    }

    [Fact]
    public async Task GetUnreadAsync_ReturnsOnlyUnreadNotifications()
    {
        // Arrange
        var unread = new List<Domain.Entities.Notification>
        {
            new NotificationBuilder().Build(),
            new NotificationBuilder().WithId("6a2a3513034b3271f27a2341").Build()
        };

        _notificationRepositoryMock
            .Setup(r => r.GetUnreadAsync())
            .ReturnsAsync(unread);

        // Act
        var result = await _sut.GetUnreadAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(n => n.IsRead.Should().BeFalse());
    }

    [Fact]
    public async Task MarkAsReadAsync_DelegatesToRepository()
    {
        // Arrange
        var notificationId = "6a2a3513034b3271f27a233f";
        var userId = "6a29ccb85c6f09702e1853de";

        _notificationRepositoryMock
            .Setup(r => r.MarkAsReadAsync(notificationId, userId))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.MarkAsReadAsync(notificationId, userId);

        // Assert
        _notificationRepositoryMock.Verify(
            r => r.MarkAsReadAsync(notificationId, userId),
            Times.Once);
    }

    [Fact]
    public async Task SendAsync_CreatesNotificationWithCorrectTimestamp()
    {
        // Arrange
        Domain.Entities.Notification? capturedNotification = null;

        _notificationRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<Domain.Entities.Notification>()))
            .Callback<Domain.Entities.Notification>(n => capturedNotification = n)
            .Returns(Task.CompletedTask);

        _notificationDispatcherMock
            .Setup(d => d.SendNotificationAsync(It.IsAny<Domain.Entities.Notification>()))
            .Returns(Task.CompletedTask);

        var before = DateTime.UtcNow;

        // Act
        await _sut.SendAsync("Test message", "orderId123", NotificationType.OrderRegistered);

        // Assert
        capturedNotification.Should().NotBeNull();
        capturedNotification!.CreatedAt.Should().BeOnOrAfter(before);
        capturedNotification.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }
}