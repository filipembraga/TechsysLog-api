using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TechsysLog.Domain.Entities;

public class Notification
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;
    public string Message { get; set; } = null!;
    public string OrderId { get; set; } = null!;
    public bool IsRead { get; set; }
    public DateTime ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }
}