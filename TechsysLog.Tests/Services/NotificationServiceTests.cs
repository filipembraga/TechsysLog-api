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
    public async Task SendAsync_BuildsNotificationWithAllFieldsAndDispatches()
    {
        // Arrange
        Domain.Entities.Notification? captured = null;

        _notificationRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<Domain.Entities.Notification>()))
            .Callback<Domain.Entities.Notification>(n => captured = n)
            .Returns(Task.CompletedTask);

        _notificationDispatcherMock
            .Setup(d => d.SendNotificationAsync(It.IsAny<Domain.Entities.Notification>()))
            .Returns(Task.CompletedTask);

        var before = DateTime.UtcNow;

        // Act
        await _sut.SendAsync("New order ORD-00001 registered.", "orderId123", NotificationType.OrderRegistered);

        // Assert
        captured.Should().NotBeNull();
        captured!.Message.Should().Be("New order ORD-00001 registered.");
        captured.OrderId.Should().Be("orderId123");
        captured.Type.Should().Be(NotificationType.OrderRegistered);
        captured.IsRead.Should().BeFalse();
        captured.CreatedAt.Should().BeOnOrAfter(before);
        captured.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));

        _notificationRepositoryMock.Verify(
            r => r.CreateAsync(It.IsAny<Domain.Entities.Notification>()), Times.Once);
        _notificationDispatcherMock.Verify(
            d => d.SendNotificationAsync(captured), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllNotificationsMapped()
    {
        // Arrange
        var readAt = DateTime.UtcNow;
        var notifications = new List<Domain.Entities.Notification>
    {
        new NotificationBuilder()
            .WithId("6a2a3513034b3271f27a233f")
            .WithMessage("New order ORD-00001 registered.")
            .WithType(NotificationType.OrderRegistered)
            .Build(),
        new NotificationBuilder()
            .WithId("6a2a3513034b3271f27a2340")
            .WithMessage("Delivery registered for ORD-00002.")
            .WithType(NotificationType.OrderDelivered)
            .AsRead()
            .WithReadAt(readAt)
            .Build()
    };

        _notificationRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(notifications);

        // Act
        var result = await _sut.GetAllAsync();

        // Assert — every mapped field, not just Count/IsRead
        result.Should().HaveCount(2);

        result[0].Id.Should().Be("6a2a3513034b3271f27a233f");
        result[0].Message.Should().Be("New order ORD-00001 registered.");
        result[0].Type.Should().Be(NotificationType.OrderRegistered);
        result[0].IsRead.Should().BeFalse();
        result[0].ReadAt.Should().BeNull();

        result[1].Id.Should().Be("6a2a3513034b3271f27a2340");
        result[1].Message.Should().Be("Delivery registered for ORD-00002.");
        result[1].Type.Should().Be(NotificationType.OrderDelivered);
        result[1].IsRead.Should().BeTrue();
        result[1].ReadAt.Should().Be(readAt);
    }

    [Fact]
    public async Task GetUnreadAsync_ReturnsOnlyUnreadNotificationsMapped()
    {
        // Arrange
        var unread = new List<Domain.Entities.Notification>
    {
        new NotificationBuilder()
            .WithId("6a2a3513034b3271f27a2341")
            .WithMessage("New order ORD-00003 registered.")
            .WithType(NotificationType.OrderRegistered)
            .Build()
    };

        _notificationRepositoryMock
            .Setup(r => r.GetUnreadAsync())
            .ReturnsAsync(unread);

        // Act
        var result = await _sut.GetUnreadAsync();

        // Assert
        result.Should().HaveCount(1);
        result[0].Id.Should().Be("6a2a3513034b3271f27a2341");
        result[0].Message.Should().Be("New order ORD-00003 registered.");
        result[0].Type.Should().Be(NotificationType.OrderRegistered);
        result[0].IsRead.Should().BeFalse();
        result[0].ReadAt.Should().BeNull();
    }
}