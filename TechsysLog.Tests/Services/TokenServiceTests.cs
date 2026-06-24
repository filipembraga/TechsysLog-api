using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Options;
using TechsysLog.Application.Services;
using TechsysLog.Application.Settings;

namespace TechsysLog.Tests.Services;

/// <summary>
/// Unit tests for TokenService.
/// Verifies that generated tokens carry the correct claims, issuer, audience and expiration.
/// These are security-critical contracts — a silent regression here breaks authentication for all users.
/// </summary>
public class TokenServiceTests
{
    private const string UserId = "6a29ccb85c6f09702e1853de";
    private const string Email = "filipe@techsyslog.com";

    private static TokenService Create(int expirationHours = 1) => new(Options.Create(new JwtSettings
    {
        Secret = "supersecretkey-minimum-32-characters-long!!",
        Issuer = "TechsysLog",
        Audience = "TechsysLogUsers",
        ExpirationHours = expirationHours
    }));

    [Fact]
    public void GenerateToken_ReturnsWellFormedJwt()
    {
        // JWT format is three base64url segments separated by dots
        var token = Create().GenerateToken(UserId, Email);
        token.Split('.').Should().HaveCount(3);
    }

    [Fact]
    public void GenerateToken_ContainsUserIdClaim()
    {
        var token = Create().GenerateToken(UserId, Email);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        jwt.Claims.Should().Contain(c =>
            c.Type == ClaimTypes.NameIdentifier && c.Value == UserId);
    }

    [Fact]
    public void GenerateToken_ContainsEmailClaim()
    {
        var token = Create().GenerateToken(UserId, Email);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        jwt.Claims.Should().Contain(c =>
            c.Type == ClaimTypes.Email && c.Value == Email);
    }

    [Fact]
    public void GenerateToken_HasCorrectIssuer()
    {
        var token = Create().GenerateToken(UserId, Email);
        new JwtSecurityTokenHandler().ReadJwtToken(token).Issuer.Should().Be("TechsysLog");
    }

    [Fact]
    public void GenerateToken_HasCorrectAudience()
    {
        var token = Create().GenerateToken(UserId, Email);
        new JwtSecurityTokenHandler().ReadJwtToken(token).Audiences.Should().Contain("TechsysLogUsers");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(24)]
    public void GenerateToken_ExpiresAfterConfiguredHours(int hours)
    {
        var before = DateTime.UtcNow;
        var token = Create(expirationHours: hours).GenerateToken(UserId, Email);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        jwt.ValidTo.Should().BeCloseTo(before.AddHours(hours), TimeSpan.FromSeconds(5));
    }

}
