namespace TechsysLog.Infrastructure.Repositories
{
    using System.Linq;
    using System.Threading.Tasks;
    using MongoDB.Driver;
    using TechsysLog.Domain.Interfaces;
    using TechsysLog.Domain.Entities;
    using TechsysLog.Infrastructure.Context;

    public class DeliveryRepository : IDeliveryRepository
    {
        private readonly IMongoCollection<Delivery> _collection;

        public DeliveryRepository(MongoDbContext dbContext)
        {
            _collection = dbContext.Deliveries;
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
}