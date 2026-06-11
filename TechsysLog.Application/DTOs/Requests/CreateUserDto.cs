using System.Diagnostics.CodeAnalysis;

namespace TechsysLog.Application.DTOs.Requests;

[ExcludeFromCodeCoverage]
public class CreateUserDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}