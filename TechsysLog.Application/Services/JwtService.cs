using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TechsysLog.Application.Interfaces;
using TechsysLog.Application.Settings;

namespace TechsysLog.Application.Services;

/// <summary>
/// Generates JWT tokens for authenticated users.
/// Design decision: token expiration is set to 1 hour.
/// Refresh tokens were not implemented given the project scope —
/// in production, a refresh token strategy would be required.
/// </summary>
public class JwtService : IJwtService
{
    private readonly JwtSettings _settings;

    public JwtService(IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;
    }

    public string GenerateToken(string userId, string email)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, email)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_settings.Secret));

        var credentials = new SigningCredentials(
            key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(_settings.ExpirationHours),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}