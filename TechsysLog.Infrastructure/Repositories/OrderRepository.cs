namespace TechsysLog.Infrastructure.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using MongoDB.Driver;
    using TechsysLog.Domain.Entities;
    using TechsysLog.Domain.Enums;
    using TechsysLog.Domain.Interfaces;
    using TechsysLog.Infrastructure.Context;

    /// Design decision: a generic BaseRepository<T> was considered but rejected.
    /// Each repository only implements the methods its entity actually needs.
    public class OrderRepository : IOrderRepository
    {
        private readonly IMongoCollection<Order> _collection;

        public OrderRepository(MongoDbContext dbContext)
        {
            _collection = dbContext.Orders;
        }

        public async Task<Order?> GetByOrderNumberAsync(string orderNumber)
        {
            var filter = Builders<Order>.Filter.Eq("OrderNumber", orderNumber);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<List<Order>> GetByUserIdAsync(string userId)
        {
            var filter = Builders<Order>.Filter.Eq(o => o.UserId, userId);
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task UpdateStatusAsync(string id, OrderStatus status)
        {
            var filter = Builders<Order>.Filter.Eq("Id", id);
            var update = Builders<Order>.Update.Set(o => o.Status, status);
            await _collection.UpdateOneAsync(filter, update);
        }
    }
}