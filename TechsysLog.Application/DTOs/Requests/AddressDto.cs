using System.Diagnostics.CodeAnalysis;

namespace TechsysLog.Application.DTOs.Requests;

[ExcludeFromCodeCoverage]
public class AddressDto
{
    public string ZipCode { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string Neighborhood { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
}