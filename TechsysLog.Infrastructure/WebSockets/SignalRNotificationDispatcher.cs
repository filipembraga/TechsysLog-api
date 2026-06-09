using Microsoft.AspNetCore.SignalR;
using TechsysLog.Domain.Entities;
using TechsysLog.Domain.Interfaces;
using TechsysLog.Infrastructure.WebSockets.Hubs;

namespace TechsysLog.Infrastructure.WebSockets;

public class SignalRNotificationDispatcher : INotificationDispatcher
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public SignalRNotificationDispatcher(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendNotificationAsync(Notification notification)
    {
        await _hubContext.Clients.All.SendAsync("ReceiveNotification", notification);
    }
}