using TechsysLog.Application.DTOs.Responses;
using TechsysLog.Application.Interfaces;
using TechsysLog.Domain.Entities;
using TechsysLog.Domain.Enums;
using TechsysLog.Domain.Interfaces;

namespace TechsysLog.Application.Services;

/// <summary>
/// Handles notification creation, retrieval, and real-time dispatch via SignalR.
///
/// Design decision: notifications are broadcast to all connected clients
/// rather than targeted to a specific user. This reflects the logistics
/// context where order and delivery updates are relevant to the operations team as a whole.
///
/// Design decision: notifications are never deleted — only marked as read.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationDispatcher _notificationDispatcher;

    public NotificationService(
        INotificationRepository notificationRepository,
        INotificationDispatcher notificationDispatcher)
    {
        _notificationRepository = notificationRepository;
        _notificationDispatcher = notificationDispatcher;
    }

    public async Task SendAsync(string message, string orderId, NotificationType type)
    {
        var notification = new Notification
        {
            Message = message,
            OrderId = orderId,
            Type = type,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        await _notificationRepository.CreateAsync(notification);

        await _notificationDispatcher.SendNotificationAsync(notification);
    }

    public async Task<List<NotificationResponseDto>> GetAllAsync()
    {
        var notifications = await _notificationRepository.GetAllAsync();
        return notifications.Select(MapToResponse).ToList();
    }

    public async Task<List<NotificationResponseDto>> GetUnreadAsync()
    {
        var notifications = await _notificationRepository.GetUnreadAsync();
        return notifications.Select(MapToResponse).ToList();
    }

    public async Task MarkAsReadAsync(string id, string userId)
    {
        await _notificationRepository.MarkAsReadAsync(id, userId);
    }

    private static NotificationResponseDto MapToResponse(Notification notification) => new()
    {
        Id = notification.Id,
        Message = notification.Message,
        OrderId = notification.OrderId,
        Type = notification.Type,
        IsRead = notification.IsRead,
        CreatedAt = notification.CreatedAt,
        ReadAt = notification.ReadAt
    };
}