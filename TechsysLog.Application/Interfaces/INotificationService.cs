using TechsysLog.Application.DTOs.Responses;

namespace TechsysLog.Application.Interfaces;

public interface INotificationService
{
    Task SendAsync(string message, string orderId);
    Task<List<NotificationResponseDto>> GetAllAsync();
    Task<List<NotificationResponseDto>> GetUnreadAsync();
    Task MarkAsReadAsync(string id, string userId); 
}