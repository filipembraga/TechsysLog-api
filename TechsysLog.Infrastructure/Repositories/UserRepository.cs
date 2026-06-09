using MongoDB.Driver;
using TechsysLog.Domain.Entities;
using TechsysLog.Domain.Interfaces;
using TechsysLog.Infrastructure.Context;

namespace TechsysLog.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _collection;

    public UserRepository(MongoDbContext dbContext)
    {
        _collection = dbContext.Users;
    }

    public async Task CreateAsync(User user)
    {
        await _collection.InsertOneAsync(user);
    }
    
    public async Task<User?> GetByEmailAsync(string email)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Email, email);
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }
}