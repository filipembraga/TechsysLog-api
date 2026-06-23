using MongoDB.Bson;

namespace TechsysLog.Domain.Entities;

public class RefreshToken
{
    public ObjectId Id { get; set; }
    public ObjectId UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
}