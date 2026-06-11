using System.Diagnostics.CodeAnalysis;

namespace TechsysLog.Application.DTOs.Requests;

[ExcludeFromCodeCoverage]
public class CreateOrderDto
{
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public AddressDto DeliveryAddress { get; set; } = new();
}