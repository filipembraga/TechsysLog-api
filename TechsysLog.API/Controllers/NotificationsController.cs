using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechsysLog.Application.DTOs.Responses;
using TechsysLog.Application.Interfaces;

namespace TechsysLog.API.Controllers;

/// <summary>
/// Handles notification retrieval and read status management.
///
/// Design decision: notifications are broadcast to all connected clients
/// rather than targeted per user. The read status is tracked per user
/// via the ReadByUserId field — a user marking a notification as read
/// does not affect other users' unread counts.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    /// <summary>
    /// Returns all notifications in the system.
    /// </summary>
    /// <response code="200">List of notifications returned (may be empty)</response>
    /// <response code="401">User is not authenticated</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<NotificationResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllAsync()
    {
        var notifications = await _notificationService.GetAllAsync();
        return Ok(notifications);
    }

    /// <summary>
    /// Returns only unread notifications.
    /// </summary>
    /// <response code="200">List of unread notifications returned (may be empty)</response>
    /// <response code="401">User is not authenticated</response>
    [HttpGet("unread")]
    [ProducesResponseType(typeof(List<NotificationResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUnreadAsync()
    {
        var notifications = await _notificationService.GetUnreadAsync();
        return Ok(notifications);
    }

    /// <summary>
    /// Marks a notification as read for the authenticated user.
    /// </summary>
    /// <param name="id">The notification's MongoDB ObjectId</param>
    /// <response code="204">Notification marked as read</response>
    /// <response code="401">User is not authenticated</response>
    /// <response code="404">Notification not found</response>
    [HttpPatch("{id}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsReadAsync(string id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User identity not found in token.");

        await _notificationService.MarkAsReadAsync(id, userId);
        return NoContent();
    }
}