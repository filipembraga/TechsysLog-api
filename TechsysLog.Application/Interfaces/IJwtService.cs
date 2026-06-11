
namespace TechsysLog.Application.Interfaces;

/// <summary>
/// Contract for JWT token generation.
/// Separated from UserService to respect Single Responsibility
/// </summary>
public interface IJwtService
{
    string GenerateToken(string userId, string email);
}