using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TechsysLog.API.Controllers;
using TechsysLog.Application.DTOs.Requests;
using TechsysLog.Application.DTOs.Responses;
using TechsysLog.Application.Interfaces;

namespace TechsysLog.Tests.Controllers;

/// <summary>
/// Unit tests for DeliveriesController.
/// Exception propagation tests verify that ExceptionHandlingMiddleware.
/// </summary>
public class DeliveriesControllerTests
{
    private readonly Mock<IDeliveryService> _deliveryServiceMock;
    private readonly DeliveriesController _sut;

    public DeliveriesControllerTests()
    {
        _deliveryServiceMock = new Mock<IDeliveryService>();
        _sut = new DeliveriesController(_deliveryServiceMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_WithValidOrder_Returns201()
    {
        // Arrange
        var dto = new CreateDeliveryDto
        {
            OrderId = "6a2a344d034b3271f27a233c"
        };

        var responseDto = new DeliveryResponseDto
        {
            Id = "6a2a3513034b3271f27a233e",
            OrderId = dto.OrderId,
            DeliveredAt = DateTime.UtcNow
        };

        _deliveryServiceMock
            .Setup(s => s.RegisterAsync(dto))
            .ReturnsAsync(responseDto);

        // Act
        var result = await _sut.RegisterAsync(dto);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
        createdResult.Value.Should().BeEquivalentTo(responseDto);
    }

    [Fact]
    public async Task GetByOrderIdAsync_WithExistingDelivery_Returns200()
    {
        // Arrange
        var orderId = "6a2a344d034b3271f27a233c";
        var responseDto = new DeliveryResponseDto
        {
            Id = "6a2a3513034b3271f27a233e",
            OrderId = orderId,
            DeliveredAt = DateTime.UtcNow
        };

        _deliveryServiceMock
            .Setup(s => s.GetByOrderIdAsync(orderId))
            .ReturnsAsync(responseDto);

        // Act
        var result = await _sut.GetByOrderIdAsync(orderId);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(responseDto);
    }

    [Fact]
    public async Task GetByOrderIdAsync_WithNoDelivery_Returns404()
    {
        // Arrange
        _deliveryServiceMock
            .Setup(s => s.GetByOrderIdAsync(It.IsAny<string>()))
            .ReturnsAsync((DeliveryResponseDto?)null);

        // Act
        var result = await _sut.GetByOrderIdAsync("orderWithNoDelivery");

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.StatusCode.Should().Be(404);
    }
}