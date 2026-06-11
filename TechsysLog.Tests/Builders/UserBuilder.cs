using TechsysLog.Domain.Entities;

namespace TechsysLog.Tests.Builders;

/// <summary>
/// Test data builder for User entity that provides sensible defaults that can be overridden per test.
/// </summary>
public class UserBuilder
{
    private string _id = "6a29ccb85c6f09702e1853de";
    private string _name = "Test User";
    private string _email = "test@techsyslog.com";
    private string _passwordHash = BCrypt.Net.BCrypt.HashPassword("Test@1234");

    public UserBuilder WithId(string id) { _id = id; return this; }
    public UserBuilder WithName(string name) { _name = name; return this; }
    public UserBuilder WithEmail(string email) { _email = email; return this; }
    public UserBuilder WithPasswordHash(string hash) { _passwordHash = hash; return this; }

    public User Build() => new()
    {
        Id = _id,
        Name = _name,
        Email = _email,
        PasswordHash = _passwordHash,
        CreatedAt = DateTime.UtcNow
    };
}