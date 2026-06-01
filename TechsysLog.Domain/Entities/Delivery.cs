namespace TechsysLog.Domain.Entities;

public class Delivery
{
    public Guid Id { get; set; }
    public string OrderId { get; set; } = null!;
    public DateTime DeliveryDate { get; set; }
}