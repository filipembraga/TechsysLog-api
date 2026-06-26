using TechsysLog.Domain.Entities;
using TechsysLog.Domain.Enums;

namespace TechsysLog.Tests.Builders;

/// <summary>
/// Test data builder for Notification entity that provides sensible defaults that can be overridden per test.
/// </summary>
public class NotificationBuilder
{
    private string _id = "6a2a3513034b3271f27a233f";
    private string _message = "New order ORD-00001 registered.";
    private string _orderId = "6a2a344d034b3271f27a233c";
    private NotificationType _type = NotificationType.OrderRegistered;
    private bool _isRead = false;
    private DateTime? _readAt = null;

    public NotificationBuilder WithId(string id) { _id = id; return this; }
    public NotificationBuilder WithMessage(string message) { _message = message; return this; }
    public NotificationBuilder WithType(NotificationType type) { _type = type; return this; }
    public NotificationBuilder AsRead() { _isRead = true; return this; }
    public NotificationBuilder WithReadAt(DateTime? readAt) { _readAt = readAt; return this; }

    public Notification Build() => new()
    {
        Id = _id,
        Message = _message,
        OrderId = _orderId,
        Type = _type,
        IsRead = _isRead,
        ReadAt = _readAt,
        CreatedAt = DateTime.UtcNow
    };
}