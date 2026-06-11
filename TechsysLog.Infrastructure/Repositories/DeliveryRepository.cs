using MongoDB.Driver;
using TechsysLog.Domain.Interfaces;
using TechsysLog.Domain.Entities;
using TechsysLog.Infrastructure.Context;
using System.Diagnostics.CodeAnalysis;

namespace TechsysLog.Infrastructure.Repositories;

[ExcludeFromCodeCoverage]
public class DeliveryRepository : IDeliveryRepository
{
    private readonly IMongoCollection<Delivery> _collection;

    public DeliveryRepository(MongoDbContext dbContext)
    {
        _collection = dbContext.Deliveries;
    }

    public async Task CreateAsync(Delivery delivery)
    {
        await _collection.InsertOneAsync(delivery);
    }

    public async Task<Delivery?> GetByOrderIdAsync(string orderId)
    {
        var filter = Builders<Delivery>.Filter.Eq(d => d.OrderId, orderId);
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<bool> OrderAlreadyDeliveredAsync(string orderId)
    {
        return await _collection.Find(d => d.OrderId == orderId).AnyAsync();
    }
}