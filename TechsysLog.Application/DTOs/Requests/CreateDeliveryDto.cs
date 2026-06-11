using System.Diagnostics.CodeAnalysis;

namespace TechsysLog.Application.DTOs.Requests;

/// <summary>
/// DTO for registering a delivery.
///
/// Design decision: DeliveredAt is nullable — if not provided by the caller,
/// the service captures DateTime.UtcNow at the moment of registration.
/// This prevents client-side clock manipulation while still allowing
/// back-dated registrations when necessary.
/// </summary>
[ExcludeFromCodeCoverage]
public class CreateDeliveryDto
{
    public string OrderId { get; set; } = string.Empty;
    public DateTime? DeliveredAt { get; set; }
}