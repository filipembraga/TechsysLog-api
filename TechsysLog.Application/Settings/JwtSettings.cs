using System.Diagnostics.CodeAnalysis;

namespace TechsysLog.Application.Settings;

/// <summary>
/// Configuration for JWT token generation.
/// Design decision: JwtSettings lives in the Application layer rather than
/// Infrastructure because JWT token generation is an application concern —
/// </summary>
[ExcludeFromCodeCoverage]
public class JwtSettings
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int AccessTokenExpirationMinutes { get; set; }
}