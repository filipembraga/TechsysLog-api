using TechsysLog.Domain.Entities;

namespace TechsysLog.Domain.Interfaces;

public interface INotificationDispatcher
{
    Task SendNotificationAsync(Notification notification);
}