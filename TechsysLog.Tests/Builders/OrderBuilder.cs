using TechsysLog.Domain.Entities;
using TechsysLog.Domain.Enums;
using TechsysLog.Domain.ValueObjects;

namespace TechsysLog.Tests.Builders;

/// <summary>
/// Test data builder for Order entity that provides sensible defaults that can be overridden per test.
/// </summary>
public class OrderBuilder
{
    private string _id = "6a2a344d034b3271f27a233c";
    private string _orderNumber = "ORD-00001";
    private string _description = "Test Product";
    private decimal _amount = 100.00m;
    private string _userId = "6a29ccb85c6f09702e1853de";
    private OrderStatus _status = OrderStatus.Pending;
    private Address _address = new("01310-100", "Avenida Paulista", "1578",
        "Bela Vista", "São Paulo", "SP");

    public OrderBuilder WithId(string id) { _id = id; return this; }
    public OrderBuilder WithOrderNumber(string number) { _orderNumber = number; return this; }
    public OrderBuilder WithStatus(OrderStatus status) { _status = status; return this; }
    public OrderBuilder WithUserId(string userId) { _userId = userId; return this; }
    public OrderBuilder WithAmount(decimal amount) { _amount = amount; return this; }

    public Order Build() => new()
    {
        Id = _id,
        OrderNumber = _orderNumber,
        Description = _description,
        Amount = _amount,
        UserId = _userId,
        Status = _status,
        DeliveryAddress = _address,
        CreatedAt = DateTime.UtcNow
    };
}