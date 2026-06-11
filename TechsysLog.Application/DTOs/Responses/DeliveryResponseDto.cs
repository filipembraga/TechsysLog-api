using System.Diagnostics.CodeAnalysis;

namespace TechsysLog.Application.DTOs.Responses;

[ExcludeFromCodeCoverage]
public class DeliveryResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public DateTime DeliveredAt { get; set; }
}