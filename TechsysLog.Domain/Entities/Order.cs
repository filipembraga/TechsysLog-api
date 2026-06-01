using TechsysLog.Domain.ValueObjects;
using TechsysLog.Domain.Enums;

namespace TechsysLog.Domain.Entities;

public class Order
{
    public Guid Id { get; set; }
    public int OrderNumber { get; set; }
    public string Description { get; set; } = null!;
    public Address DeliveryAddress { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public string UserId { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}