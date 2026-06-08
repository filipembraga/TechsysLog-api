namespace TechsysLog.Domain.Interfaces
{
    using System.Threading.Tasks;
    using TechsysLog.Domain.Entities;

    public interface INotificationRepository
    {
        Task<List<Notification>> GetUnreadAsync();
        Task MarkAsReadAsync(string id, string userId);
    }
}