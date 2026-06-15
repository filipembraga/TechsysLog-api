using FluentAssertions;
using Moq;
using TechsysLog.Application.DTOs.Requests;
using TechsysLog.Application.Interfaces;
using TechsysLog.Application.Services;
using TechsysLog.Domain.Enums;
using TechsysLog.Domain.Interfaces;
using TechsysLog.Tests.Builders;

namespace TechsysLog.Tests.Services;

/// <summary>
/// Unit tests for DeliveryService.
/// </summary>
public class DeliveryServiceTests
{
    private readonly Mock<IDeliveryRepository> _deliveryRepositoryMock;
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly IDeliveryService _sut;

    public DeliveryServiceTests()
    {
        _deliveryRepositoryMock = new Mock<IDeliveryRepository>();
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _notificationServiceMock = new Mock<INotificationService>();

        _sut = new DeliveryService(
            _deliveryRepositoryMock.Object,
            _orderRepositoryMock.Object,
            _notificationServiceMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_WithValidOrder_ReturnsDeliveryResponseDto()
    {
        // Arrange
        var order = new OrderBuilder()
            .WithStatus(OrderStatus.Pending)
            .Build();

        var dto = new CreateDeliveryDto { OrderId = order.Id };

        _orderRepositoryMock.Setup(r => r.GetByIdAsync(order.Id)).ReturnsAsync(order);
        _deliveryRepositoryMock.Setup(r => r.OrderAlreadyDeliveredAsync(order.Id)).ReturnsAsync(false);
        _deliveryRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Domain.Entities.Delivery>()))
            .Returns(Task.CompletedTask);
        _orderRepositoryMock.Setup(r => r.UpdateStatusAsync(order.Id, OrderStatus.Delivered))
            .Returns(Task.CompletedTask);
        _notificationServiceMock.Setup(n => n.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NotificationType>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.RegisterAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.OrderId.Should().Be(order.Id);
        result.DeliveredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task RegisterAsync_WithNonExistentOrder_ThrowsKeyNotFoundException()
    {
        // Arrange
        _orderRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((Domain.Entities.Order?)null);

        var dto = new CreateDeliveryDto { OrderId = "nonexistentid" };

        // Act
        var act = async () => await _sut.RegisterAsync(dto);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task RegisterAsync_WithAlreadyDeliveredOrder_ThrowsInvalidOperationException()
    {
        // Arrange
        var order = new OrderBuilder()
            .WithStatus(OrderStatus.Delivered)
            .Build();

        _orderRepositoryMock.Setup(r => r.GetByIdAsync(order.Id)).ReturnsAsync(order);

        var dto = new CreateDeliveryDto { OrderId = order.Id };

        // Act
        var act = async () => await _sut.RegisterAsync(dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already been delivered*");
    }

    [Fact]
    public async Task RegisterAsync_UpdatesOrderStatusToDelivered()
    {
        // Arrange
        var order = new OrderBuilder().WithStatus(OrderStatus.Pending).Build();
        var dto = new CreateDeliveryDto { OrderId = order.Id };

        _orderRepositoryMock.Setup(r => r.GetByIdAsync(order.Id)).ReturnsAsync(order);
        _deliveryRepositoryMock.Setup(r => r.OrderAlreadyDeliveredAsync(order.Id)).ReturnsAsync(false);
        _deliveryRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Domain.Entities.Delivery>()))
            .Returns(Task.CompletedTask);
        _orderRepositoryMock.Setup(r => r.UpdateStatusAsync(It.IsAny<string>(), It.IsAny<OrderStatus>()))
            .Returns(Task.CompletedTask);
        _notificationServiceMock.Setup(n => n.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NotificationType>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.RegisterAsync(dto);

        // Assert — status update was called with Delivered
        _orderRepositoryMock.Verify(
            r => r.UpdateStatusAsync(order.Id, OrderStatus.Delivered),
            Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WithProvidedDeliveredAt_UsesProvidedDateTime()
    {
        // Arrange
        var order = new OrderBuilder().WithStatus(OrderStatus.Pending).Build();
        var specificDate = new DateTime(2026, 6, 11, 10, 0, 0, DateTimeKind.Utc);
        var dto = new CreateDeliveryDto { OrderId = order.Id, DeliveredAt = specificDate };

        _orderRepositoryMock.Setup(r => r.GetByIdAsync(order.Id)).ReturnsAsync(order);
        _deliveryRepositoryMock.Setup(r => r.OrderAlreadyDeliveredAsync(order.Id)).ReturnsAsync(false);
        _deliveryRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Domain.Entities.Delivery>()))
            .Returns(Task.CompletedTask);
        _orderRepositoryMock.Setup(r => r.UpdateStatusAsync(It.IsAny<string>(), It.IsAny<OrderStatus>()))
            .Returns(Task.CompletedTask);
        _notificationServiceMock.Setup(n => n.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NotificationType>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.RegisterAsync(dto);

        // Assert — user-provided date is respected
        result.DeliveredAt.Should().Be(specificDate);
    }

    [Fact]
    public async Task RegisterAsync_WhenDeliveryRecordAlreadyExists_ThrowsInvalidOperationException()
    {
        // Covers the secondary duplicate guard: order status is Pending but a delivery
        // record already exists in the database. Both guards must be independent.
        var order = new OrderBuilder().WithStatus(OrderStatus.Pending).Build();
        _orderRepositoryMock.Setup(r => r.GetByIdAsync(order.Id)).ReturnsAsync(order);
        _deliveryRepositoryMock.Setup(r => r.OrderAlreadyDeliveredAsync(order.Id)).ReturnsAsync(true);

        var act = async () => await _sut.RegisterAsync(new CreateDeliveryDto { OrderId = order.Id });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*{order.OrderNumber}*");
    }

    [Fact]
    public async Task GetByOrderIdAsync_WhenDeliveryExists_ReturnsMappedDto()
    {
        // Arrange
        var delivery = new Domain.Entities.Delivery
        {
            Id = "deliveryid",
            OrderId = "orderid",
            DeliveryDate = new DateTime(2026, 6, 11, 10, 0, 0, DateTimeKind.Utc)
        };
        _deliveryRepositoryMock.Setup(r => r.GetByOrderIdAsync("orderid")).ReturnsAsync(delivery);

        // Act
        var result = await _sut.GetByOrderIdAsync("orderid");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be("deliveryid");
        result.OrderId.Should().Be("orderid");
        result.DeliveredAt.Should().Be(delivery.DeliveryDate);
    }

    [Fact]
    public async Task GetByOrderIdAsync_WhenDeliveryNotFound_ReturnsNull()
    {
        _deliveryRepositoryMock.Setup(r => r.GetByOrderIdAsync(It.IsAny<string>()))
            .ReturnsAsync((Domain.Entities.Delivery?)null);

        var result = await _sut.GetByOrderIdAsync("nonexistentorderid");

        result.Should().BeNull();
    }
}