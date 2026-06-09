using TechsysLog.Application.DTOs.Requests;
using TechsysLog.Domain.Enums;

namespace TechsysLog.Application.DTOs.Responses;

public class OrderResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string OrderNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public AddressDto DeliveryAddress { get; set; } = new();
    public OrderStatus Status { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}