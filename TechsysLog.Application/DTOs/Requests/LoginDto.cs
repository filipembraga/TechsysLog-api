using System.Diagnostics.CodeAnalysis;

namespace TechsysLog.Application.DTOs.Requests;

[ExcludeFromCodeCoverage]
public class LoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}