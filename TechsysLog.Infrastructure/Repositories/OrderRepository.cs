using MongoDB.Bson;
using MongoDB.Driver;
using TechsysLog.Domain.Entities;
using TechsysLog.Domain.Enums;
using TechsysLog.Domain.Interfaces;
using TechsysLog.Infrastructure.Context;

namespace TechsysLog.Infrastructure.Repositories;

/// Design decision: a generic BaseRepository<T> was considered but rejected.
/// Each repository only implements the methods its entity actually needs.
public class OrderRepository : IOrderRepository
{
    private readonly IMongoCollection<Order> _collection;

    public OrderRepository(MongoDbContext dbContext)
    {
        _collection = dbContext.Orders;
    }

    public async Task CreateAsync(Order order)
    {
        await _collection.InsertOneAsync(order);
    }

    public async Task<Order?> GetByIdAsync(string id)
    {
        var filter = Builders<Order>.Filter.Eq(o => o.Id, id);
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<Order?> GetByOrderNumberAsync(string orderNumber)
    {
        var filter = Builders<Order>.Filter.Eq("OrderNumber", orderNumber);
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<List<Order>> GetAllByUserIdAsync(string userId)
    {
        var filter = Builders<Order>.Filter.Eq("UserId", userId);
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task UpdateStatusAsync(string id, OrderStatus status)
    {
        var filter = Builders<Order>.Filter.Eq(o => o.Id, id);
        var update = Builders<Order>.Update.Set(o => o.Status, status);
        await _collection.UpdateOneAsync(filter, update);
    }

    public async Task<long> CountAsync()
    {
        return await _collection.CountDocumentsAsync(_ => true);
    }   
}
