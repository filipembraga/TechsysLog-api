using TechsysLog.Application.DTOs.Responses;
using TechsysLog.Domain.Enums;

namespace TechsysLog.Application.Interfaces;

public interface INotificationService
{
    Task SendAsync(string message, string orderId, NotificationType type);
    Task<List<NotificationResponseDto>> GetAllAsync();
    Task<List<NotificationResponseDto>> GetUnreadAsync();
    Task MarkAsReadAsync(string id, string userId); 
}