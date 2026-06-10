using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechsysLog.Application.DTOs.Requests;
using TechsysLog.Application.DTOs.Responses;
using TechsysLog.Application.Interfaces;

namespace TechsysLog.API.Controllers;

/// <summary>
/// Handles delivery registration and retrieval.
/// Registering a delivery automatically updates the related order status to Delivered and dispatches a real-time notification via SignalR.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class DeliveriesController : ControllerBase
{
    private readonly IDeliveryService _deliveryService;

    public DeliveriesController(IDeliveryService deliveryService)
    {
        _deliveryService = deliveryService;
    }

    /// <summary>
    /// Registers a delivery for an existing order.
    /// The order status is automatically updated to Delivered.
    /// </summary>
    /// <param name="dto">Delivery registration data including the order ID</param>
    /// <response code="201">Delivery registered successfully</response>
    /// <response code="401">User is not authenticated</response>
    /// <response code="404">Order not found</response>
    /// <response code="409">Order has already been delivered</response>
    [HttpPost]
    [ProducesResponseType(typeof(DeliveryResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegisterAsync([FromBody] CreateDeliveryDto dto)
    {
        var delivery = await _deliveryService.RegisterAsync(dto);
        return CreatedAtAction(nameof(GetByOrderIdAsync), new { orderId = delivery.OrderId }, delivery);
    }

    /// <summary>
    /// Returns the delivery record for a specific order.
    /// </summary>
    /// <param name="orderId">The order's MongoDB ObjectId</param>
    /// <response code="200">Delivery found and returned</response>
    /// <response code="401">User is not authenticated</response>
    /// <response code="404">No delivery found for this order</response>
    [HttpGet("{orderId}")]
    [ProducesResponseType(typeof(DeliveryResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByOrderIdAsync(string orderId)
    {
        var delivery = await _deliveryService.GetByOrderIdAsync(orderId);

        if (delivery is null)
            return NotFound(new { message = $"No delivery found for order {orderId}." });

        return Ok(delivery);
    }
}