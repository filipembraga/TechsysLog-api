namespace TechsysLog.Application.DTOs.Responses;

public class NotificationResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
}