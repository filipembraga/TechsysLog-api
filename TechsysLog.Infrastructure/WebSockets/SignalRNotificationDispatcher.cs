using Microsoft.AspNetCore.SignalR;
using TechsysLog.Domain.Entities;
using TechsysLog.Domain.Interfaces;
using TechsysLog.Infrastructure.WebSockets.Hubs;

namespace TechsysLog.Infrastructure.WebSockets;

/// <summary>
/// Implements real-time notification dispatch via SignalR.
/// </summary>
public class SignalRNotificationDispatcher : INotificationDispatcher
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public SignalRNotificationDispatcher(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendNotificationAsync(Notification notification)
    {
        await _hubContext.Clients.All.SendAsync("ReceiveNotification", new
        {
            id = notification.Id,
            message = notification.Message,
            orderId = notification.OrderId,
            createdAt = notification.CreatedAt
        });
    }
}