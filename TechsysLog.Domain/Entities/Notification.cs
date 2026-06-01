namespace TechsysLog.Domain.Entities;

public class Notification
{
    public Guid Id { get; set; }
    public string Message { get; set; } = null!;
    public string OrderId { get; set; } = null!;
    public bool IsRead { get; set; }
    public DateTime ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }
}