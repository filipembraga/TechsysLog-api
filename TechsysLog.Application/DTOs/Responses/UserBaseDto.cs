namespace TechsysLog.Application.DTOs.Responses;

/// <summary>
/// Base DTO containing common user properties shared across responses.
/// Avoids duplication between UserResponseDto and LoginResponseDto
/// </summary>
public class UserBaseDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
