using TechsysLog.Domain.ValueObjects;
using TechsysLog.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TechsysLog.Domain.Entities;

public class Order
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;
    public string OrderNumber { get; set; } = string.Empty;
    public string Description { get; set; } = null!;
    public Address DeliveryAddress { get; set; } = null!;
    public decimal Amount { get; set; }
    public OrderStatus Status { get; set; }
    public string UserId { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}