using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TechsysLog.Application.DTOs.Requests;
using TechsysLog.Application.Interfaces;
using TechsysLog.Application.Services;
using TechsysLog.Domain.Entities;
using TechsysLog.Domain.Enums;
using TechsysLog.Domain.Interfaces;
using TechsysLog.Domain.ValueObjects;
using TechsysLog.Tests.Builders;

namespace TechsysLog.Tests.Services;

/// <summary>
/// Unit tests for OrderService.
/// </summary>
public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IAddressLookupService> _addressLookupMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<ILogger<OrderService>> _loggerMock;
    private readonly IOrderService _sut;

    public OrderServiceTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _addressLookupMock = new Mock<IAddressLookupService>();
        _notificationServiceMock = new Mock<INotificationService>();
        _loggerMock = new Mock<ILogger<OrderService>>();

        _sut = new OrderService(
            _orderRepositoryMock.Object,
            _addressLookupMock.Object,
            _notificationServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ReturnsOrderResponseDto()
    {
        // Arrange
        var dto = new CreateOrderDto
        {
            Description = "Notebook Dell XPS",
            Amount = 8500.00m,
            DeliveryAddress = new AddressDto
            {
                ZipCode = "01310-100",
                Number = "1578",
                Street = string.Empty,
                Neighborhood = string.Empty,
                City = string.Empty,
                State = string.Empty
            }
        };

        var enrichedAddress = new Address(
            "01310-100", "Avenida Paulista", string.Empty,
            "Bela Vista", "São Paulo", "SP");

        _addressLookupMock
            .Setup(a => a.GetAddressByZipCodeAsync(dto.DeliveryAddress.ZipCode))
            .ReturnsAsync(enrichedAddress);

        _orderRepositoryMock
            .Setup(r => r.CountAsync())
            .ReturnsAsync(0);

        _orderRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<Domain.Entities.Order>()))
            .Returns(Task.CompletedTask);

        _notificationServiceMock
            .Setup(n => n.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NotificationType>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.CreateAsync(dto, "userId123");

        // Assert
        result.Should().NotBeNull();
        result.OrderNumber.Should().Be("ORD-00001");
        result.Description.Should().Be(dto.Description);
        result.DeliveryAddress.Street.Should().Be("Avenida Paulista");
        result.DeliveryAddress.Number.Should().Be("1578");
    }

    [Fact]
    public async Task CreateAsync_WhenViaCepUnavailable_FallsBackToUserProvidedAddress()
    {
        // Arrange
        var dto = new CreateOrderDto
        {
            Description = "Test Product",
            Amount = 100m,
            DeliveryAddress = new AddressDto
            {
                ZipCode = "00000-000",
                Street = "Rua Manual",
                Number = "42",
                Neighborhood = "Bairro Manual",
                City = "Cidade Manual",
                State = "MG"
            }
        };

        // ViaCEP throws — simulates service unavailability
        _addressLookupMock
            .Setup(a => a.GetAddressByZipCodeAsync(It.IsAny<string>()))
            .ThrowsAsync(new HttpRequestException("Service unavailable"));

        _orderRepositoryMock.Setup(r => r.CountAsync()).ReturnsAsync(0);
        _orderRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Domain.Entities.Order>()))
            .Returns(Task.CompletedTask);
        _notificationServiceMock.Setup(n => n.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NotificationType>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.CreateAsync(dto, "userId123");

        // Assert — fallback address is used, order is still created
        result.Should().NotBeNull();
        result.DeliveryAddress.Street.Should().Be("Rua Manual");
        result.DeliveryAddress.City.Should().Be("Cidade Manual");
    }

    [Fact]
    public async Task CreateAsync_SendsNotificationAfterCreation()
    {
        // Arrange
        var dto = new CreateOrderDto
        {
            Description = "Test",
            Amount = 50m,
            DeliveryAddress = new AddressDto
            {
                ZipCode = "01310-100",
                Number = "1",
                Street = string.Empty,
                Neighborhood = string.Empty,
                City = string.Empty,
                State = string.Empty
            }
        };

        _addressLookupMock.Setup(a => a.GetAddressByZipCodeAsync(It.IsAny<string>()))
            .ReturnsAsync((Address?)null);
        _orderRepositoryMock.Setup(r => r.CountAsync()).ReturnsAsync(4);
        _orderRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Domain.Entities.Order>()))
            .Returns(Task.CompletedTask);
        _notificationServiceMock.Setup(n => n.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NotificationType>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.CreateAsync(dto, "userId123");

        // Assert — notification was dispatched exactly once
        _notificationServiceMock.Verify(
            n => n.SendAsync(
                It.Is<string>(m => m.Contains("ORD-00005")),
                It.IsAny<string>(),
                It.IsAny<NotificationType>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ReturnsNull()
    {
        _orderRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((Domain.Entities.Order?)null);

        var result = await _sut.GetByIdAsync("nonexistentid");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByOrderNumberAsync_WithExistingOrderNumber_ReturnsMappedDto()
    {
        var order = new OrderBuilder().WithOrderNumber("ORD-00042").Build();
        _orderRepositoryMock.Setup(r => r.GetByOrderNumberAsync("ORD-00042")).ReturnsAsync(order);

        var result = await _sut.GetByOrderNumberAsync("ORD-00042");

        result.Should().NotBeNull();
        result!.OrderNumber.Should().Be("ORD-00042");
    }

    [Fact]
    public async Task GetByOrderNumberAsync_WithNonExistentOrderNumber_ReturnsNull()
    {
        _orderRepositoryMock
            .Setup(r => r.GetByOrderNumberAsync(It.IsAny<string>()))
            .ReturnsAsync((Domain.Entities.Order?)null);

        var result = await _sut.GetByOrderNumberAsync("ORD-99999");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllByUserIdAsync_ReturnsOrdersForGivenUser()
    {
        // Arrange
        var userId = "6a29ccb85c6f09702e1853de";
        var orders = new List<Order>
        {
            new OrderBuilder().WithUserId(userId).Build(),
            new OrderBuilder().WithUserId(userId).Build()
        };

        _orderRepositoryMock
            .Setup(r => r.GetAllByUserIdAsync(userId))
            .ReturnsAsync(orders);

        // Act
        var result = await _sut.GetAllByUserIdAsync(userId);

        // Assert
        result.Should().HaveCount(2);
    }
}