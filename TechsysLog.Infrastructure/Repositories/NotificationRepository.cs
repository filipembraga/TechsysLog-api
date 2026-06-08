namespace TechsysLog.Infrastructure.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using MongoDB.Driver;
    using TechsysLog.Domain.Interfaces;
    using TechsysLog.Domain.Entities;
    using TechsysLog.Infrastructure.Context;

    public class NotificationRepository : INotificationRepository
    {
        private readonly IMongoCollection<Notification> _collection;

        public NotificationRepository(MongoDbContext dbContext)
        {
            _collection = dbContext.Notifications;
        }

        public async Task<List<Notification>> GetUnreadAsync()
        {
            var filter = Builders<Notification>.Filter.Eq(n => n.IsRead, false);
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task MarkAsReadAsync(string id, string userId)
        {
            var filter = Builders<Notification>.Filter.Eq("Id", id);
            var update = Builders<Notification>.Update
                .Set(n => n.IsRead, true)
                .Set(n => n.ReadAt, DateTime.UtcNow);

            await _collection.UpdateOneAsync(filter, update);
        }
    }
}