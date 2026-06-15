using System.Diagnostics.CodeAnalysis;
using TechsysLog.Domain.Enums;

namespace TechsysLog.Application.DTOs.Responses;

[ExcludeFromCodeCoverage]
public class NotificationResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
}