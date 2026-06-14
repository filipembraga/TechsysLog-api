using System.Diagnostics.CodeAnalysis;

namespace TechsysLog.Application.DTOs.Responses;

[ExcludeFromCodeCoverage]
public class UserResponseDto : UserBaseDto
{
    public DateTime CreatedAt { get; set; }
}