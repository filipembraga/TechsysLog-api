using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TechsysLog.Domain.Entities;

public class Delivery
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;
    public string OrderId { get; set; } = null!;
    public DateTime DeliveryDate { get; set; }
}