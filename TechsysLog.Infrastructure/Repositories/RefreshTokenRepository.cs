using MongoDB.Driver;
using TechsysLog.Domain.Entities;
using TechsysLog.Domain.Interfaces;
using TechsysLog.Infrastructure.Context;

namespace TechsysLog.Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly IMongoCollection<RefreshToken> _collection;

    public RefreshTokenRepository(MongoDbContext context)
    {
        _collection = context.RefreshTokens;
    }

    public async Task CreateAsync(RefreshToken token)
    {
        await _collection.InsertOneAsync(token);
    }

    public async Task<RefreshToken?> GetByHashAsync(string tokenHash)
    {
        var filter = Builders<RefreshToken>.Filter.Eq(t => t.TokenHash, tokenHash);
        return await _collection.Find(filter).SingleOrDefaultAsync();
    }

    public async Task DeleteByHashAsync(string tokenHash)
    {
        var filter = Builders<RefreshToken>.Filter.Eq(t => t.TokenHash, tokenHash);
        await _collection.DeleteOneAsync(filter);
    }
}