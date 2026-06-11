using TechsysLog.Domain.Entities;

namespace TechsysLog.Tests.Builders;

/// <summary>
/// Test data builder for Notification entity that provides sensible defaults that can be overridden per test.
/// </summary>
public class NotificationBuilder
{
    private string _id = "6a2a3513034b3271f27a233f";
    private string _message = "New order ORD-00001 registered.";
    private string _orderId = "6a2a344d034b3271f27a233c";
    private bool _isRead = false;

    public NotificationBuilder WithId(string id) { _id = id; return this; }
    public NotificationBuilder WithMessage(string message) { _message = message; return this; }
    public NotificationBuilder AsRead() { _isRead = true; return this; }

    public Notification Build() => new()
    {
        Id = _id,
        Message = _message,
        OrderId = _orderId,
        IsRead = _isRead,
        CreatedAt = DateTime.UtcNow
    };
}