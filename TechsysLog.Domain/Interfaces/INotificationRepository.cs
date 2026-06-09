using TechsysLog.Domain.Entities;

namespace TechsysLog.Domain.Interfaces;

public interface INotificationRepository
{
    Task CreateAsync(Notification notification);
    Task<List<Notification>> GetAllAsync();
    Task<List<Notification>> GetUnreadAsync();
    Task MarkAsReadAsync(string id, string userId);
}
