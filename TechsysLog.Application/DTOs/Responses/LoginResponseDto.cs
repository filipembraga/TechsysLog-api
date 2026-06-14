using System.Diagnostics.CodeAnalysis;

namespace TechsysLog.Application.DTOs.Responses;

[ExcludeFromCodeCoverage]
public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public UserBaseDto User { get; set; } = null!;
}