using System.Security.Claims;
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
/// Unit tests for OrdersController.
/// </summary>
public class OrdersControllerTests
{
    private readonly Mock<IOrderService> _orderServiceMock;
    private readonly OrdersController _sut;
    private const string UserId = "6a29ccb85c6f09702e1853de";

    public OrdersControllerTests()
    {
        _orderServiceMock = new Mock<IOrderService>();
        _sut = new OrdersController(_orderServiceMock.Object);

        // Simulate authenticated user with JWT claims
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, UserId),
            new(ClaimTypes.Email, "filipe@techsyslog.com")
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    [Fact]
    public async Task CreateAsync_WithValidData_Returns201()
    {
        // Arrange
        var dto = new CreateOrderDto
        {
            Description = "Notebook",
            Amount = 8500m,
            DeliveryAddress = new AddressDto
            {
                ZipCode = "01310-100", Number = "1578",
                Street = string.Empty, Neighborhood = string.Empty,
                City = string.Empty, State = string.Empty
            }
        };

        var responseDto = new OrderResponseDto
        {
            Id = "6a2a344d034b3271f27a233c",
            OrderNumber = "ORD-00001",
            Description = dto.Description,
            Amount = dto.Amount,
            Status = Domain.Enums.OrderStatus.Pending,
            UserId = UserId,
            CreatedAt = DateTime.UtcNow,
            DeliveryAddress = new AddressDto
            {
                ZipCode = "01310-100", Street = "Avenida Paulista",
                Number = "1578", Neighborhood = "Bela Vista",
                City = "São Paulo", State = "SP"
            }
        };

        _orderServiceMock.Setup(s => s.CreateAsync(dto, UserId)).ReturnsAsync(responseDto);

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
        createdResult.Value.Should().BeEquivalentTo(responseDto);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_Returns404()
    {
        // Arrange
        _orderServiceMock.Setup(s => s.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((OrderResponseDto?)null);

        // Act
        var result = await _sut.GetByIdAsync("nonexistentid");

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetAllByUserIdAsync_ReturnsOrdersForAuthenticatedUser()
    {
        // Arrange
        var orders = new List<OrderResponseDto>
        {
            new() { Id = "id1", OrderNumber = "ORD-00001", UserId = UserId },
            new() { Id = "id2", OrderNumber = "ORD-00002", UserId = UserId }
        };

        _orderServiceMock.Setup(s => s.GetAllByUserIdAsync(UserId)).ReturnsAsync(orders);

        // Act
        var result = await _sut.GetAllByUserIdAsync();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(orders);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_Returns200WithOrder()
    {
        // Arrange
        var order = new OrderResponseDto { Id = "existingid", OrderNumber = "ORD-00001" };
        _orderServiceMock.Setup(s => s.GetByIdAsync("existingid")).ReturnsAsync(order);

        // Act
        var result = await _sut.GetByIdAsync("existingid");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(order);
    }

    [Fact]
    public async Task GetByOrderNumberAsync_WithExistingOrderNumber_Returns200WithOrder()
    {
        // Arrange
        var order = new OrderResponseDto { Id = "id1", OrderNumber = "ORD-00001" };
        _orderServiceMock.Setup(s => s.GetByOrderNumberAsync("ORD-00001")).ReturnsAsync(order);

        // Act
        var result = await _sut.GetByOrderNumberAsync("ORD-00001");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(order);
    }

    [Fact]
    public async Task GetByOrderNumberAsync_WithNonExistentOrderNumber_Returns404()
    {
        // Arrange
        _orderServiceMock.Setup(s => s.GetByOrderNumberAsync(It.IsAny<string>()))
            .ReturnsAsync((OrderResponseDto?)null);

        // Act
        var result = await _sut.GetByOrderNumberAsync("ORD-99999");

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.StatusCode.Should().Be(404);
    }
}