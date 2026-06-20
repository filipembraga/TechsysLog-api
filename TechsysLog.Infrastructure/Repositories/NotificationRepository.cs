using MongoDB.Driver;
using TechsysLog.Domain.Interfaces;
using TechsysLog.Domain.Entities;
using TechsysLog.Infrastructure.Context;
using System.Diagnostics.CodeAnalysis;

namespace TechsysLog.Infrastructure.Repositories;

[ExcludeFromCodeCoverage]
public class NotificationRepository : INotificationRepository
{
    private readonly IMongoCollection<Notification> _collection;

    public NotificationRepository(MongoDbContext dbContext)
    {
        _collection = dbContext.Notifications;
    }
    public async Task CreateAsync(Notification notification)
    {
        await _collection.InsertOneAsync(notification);
    }

    public async Task<List<Notification>> GetAllAsync()
    {
        return await _collection.Find(_ => true)
            .SortByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Notification>> GetUnreadAsync()
    {
        var filter = Builders<Notification>.Filter.Eq(n => n.IsRead, false);
        return await _collection.Find(filter)
            .SortByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task MarkAsReadAsync(string id, string userId)
    {
        var filter = Builders<Notification>.Filter.Eq(o => o.Id, id);
        var update = Builders<Notification>.Update
            .Set(n => n.IsRead, true)
            .Set(n => n.ReadAt, DateTime.UtcNow)
            .Set(n => n.UserId, userId );

        await _collection.UpdateOneAsync(filter, update);
    }
}