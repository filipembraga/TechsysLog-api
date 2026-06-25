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

    private static TokenService Create(int expirationMinutes = 15) => new(Options.Create(new JwtSettings
    {
        Secret = "supersecretkey-minimum-32-characters-long!!",
        Issuer = "TechsysLog",
        Audience = "TechsysLogUsers",
        AccessTokenExpirationMinutes = expirationMinutes
    }));

    [Fact]
    public void GenerateAccessToken_ReturnsWellFormedJwt()
    {
        // JWT format is three base64url segments separated by dots
        var token = Create().GenerateAccessToken(UserId, Email);
        token.Split('.').Should().HaveCount(3);
    }

    [Fact]
    public void GenerateAccessToken_ContainsUserIdClaim()
    {
        var token = Create().GenerateAccessToken(UserId, Email);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        jwt.Claims.Should().Contain(c =>
            c.Type == ClaimTypes.NameIdentifier && c.Value == UserId);
    }

    [Fact]
    public void GenerateAccessToken_ContainsEmailClaim()
    {
        var token = Create().GenerateAccessToken(UserId, Email);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        jwt.Claims.Should().Contain(c =>
            c.Type == ClaimTypes.Email && c.Value == Email);
    }

    [Fact]
    public void GenerateAccessToken_HasCorrectIssuer()
    {
        var token = Create().GenerateAccessToken(UserId, Email);
        new JwtSecurityTokenHandler().ReadJwtToken(token).Issuer.Should().Be("TechsysLog");
    }

    [Fact]
    public void GenerateAccessToken_HasCorrectAudience()
    {
        var token = Create().GenerateAccessToken(UserId, Email);
        new JwtSecurityTokenHandler().ReadJwtToken(token).Audiences.Should().Contain("TechsysLogUsers");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(24)]
    public void GenerateAccessToken_ExpiresAfterConfiguredMinutes(int minutes)
    {
        var before = DateTime.UtcNow;
        var token = Create(expirationMinutes: minutes).GenerateAccessToken(UserId, Email);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        jwt.ValidTo.Should().BeCloseTo(before.AddMinutes(minutes), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsNonEmptyValue()
    {
        var token = Create().GenerateRefreshToken();
        token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsUniqueValueOnEachCall()
    {
        var sut = Create();
        var token1 = sut.GenerateRefreshToken();
        var token2 = sut.GenerateRefreshToken();

        token1.Should().NotBe(token2);
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsValidBase64String()
    {
        var token = Create().GenerateRefreshToken();
        var act = () => Convert.FromBase64String(token);

        act.Should().NotThrow();
        Convert.FromBase64String(token).Should().HaveCount(32);
    }
}
