using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using TechsysLog.Domain.Enums;

namespace TechsysLog.Domain.Entities;

public class Notification
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;
    public string Message { get; set; } = null!;
    public string OrderId { get; set; } = null!;
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; }
    public string UserId {get; set;} = string.Empty;
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }
}