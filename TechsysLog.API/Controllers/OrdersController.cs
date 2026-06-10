using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechsysLog.Application.DTOs.Requests;
using TechsysLog.Application.DTOs.Responses;
using TechsysLog.Application.Interfaces;

namespace TechsysLog.API.Controllers;

/// <summary>
/// Handles order creation and retrieval.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Creates a new order for the authenticated user.
    /// The delivery address is automatically enriched via ViaCEP using the provided zip code.
    /// </summary>
    /// <param name="dto">Order creation data including description, amount and delivery address</param>
    /// <response code="201">Order created successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="401">User is not authenticated</response>
    [HttpPost]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateOrderDto dto)
    {
        var userId = GetUserId();
        var order = await _orderService.CreateAsync(dto, userId);

        return CreatedAtAction(nameof(GetByIdAsync), new { id = order.Id }, order);
    }

    /// <summary>
    /// Returns a single order by its internal database ID.
    /// </summary>
    /// <param name="id">The order's MongoDB ObjectId</param>
    /// <response code="200">Order found and returned</response>
    /// <response code="401">User is not authenticated</response>
    /// <response code="404">Order not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(string id)
    {
        var order = await _orderService.GetByIdAsync(id);

        if (order is null)
            return NotFound(new { message = $"Order {id} not found." });

        return Ok(order);
    }

    /// <summary>
    /// Returns a single order by its order number.
    /// </summary>
    /// <param name="orderNumber">The order number</param>
    /// <response code="200">Order found and returned</response>
    /// <response code="401">User is not authenticated</response>
    /// <response code="404">Order not found</response>
    [HttpGet("number/{orderNumber}")]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByOrderNumberAsync(string orderNumber)
    {
        var order = await _orderService.GetByOrderNumberAsync(orderNumber);

        if (order is null)
            return NotFound(new { message = $"Order {orderNumber} not found." });

        return Ok(order);
    }

    /// <summary>
    /// Returns all orders for the authenticated user.
    /// </summary>
    /// <response code="200">Orders found and returned</response>
    /// <response code="401">User is not authenticated</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrderResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllByUserIdAsync()
    {
        var userId = GetUserId();
        var orders = await _orderService.GetAllByUserIdAsync(userId);

        return Ok(orders);
    }

    /// <summary>
    /// Extracts the authenticated user's Id from the JWT token claims.
    /// </summary>
    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User identity not found in token.");
    }
}