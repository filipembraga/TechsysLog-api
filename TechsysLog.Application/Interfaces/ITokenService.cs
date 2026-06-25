
namespace TechsysLog.Application.Interfaces;

/// <summary>
/// Contract for issuing authentication tokens.
/// Separated from UserService to respect Single Responsibility
/// </summary>
public interface ITokenService
{
    string GenerateAccessToken(string userId, string email);
    string GenerateRefreshToken();
}