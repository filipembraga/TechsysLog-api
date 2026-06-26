using MongoDB.Driver;
using Microsoft.Extensions.Options;
using Techsyslog.Infrastructure.Settings;
using TechsysLog.Domain.Entities;

namespace TechsysLog.Infrastructure.Context;

/// <summary>
/// MongoDB context class responsible for managing database connections and collections.
/// </summary>
public class MongoDbContext
{
    private readonly IMongoDatabase _database;
    public IMongoClient Client { get; }

    public MongoDbContext(IOptions<MongoDbSettings> settings)
    {
        Client = new MongoClient(settings.Value.ConnectionString);
        _database = Client.GetDatabase(settings.Value.DatabaseName);
    }

    public IMongoCollection<User> Users => _database.GetCollection<User>("Users");
    public IMongoCollection<Order> Orders => _database.GetCollection<Order>("Orders");
    public IMongoCollection<Notification> Notifications => _database.GetCollection<Notification>("Notifications");
    public IMongoCollection<Delivery> Deliveries => _database.GetCollection<Delivery>("Deliveries");
    public IMongoCollection<RefreshToken> RefreshTokens => _database.GetCollection<RefreshToken>("RefreshTokens");

    public async Task EnsureIndexesCreatedAsync()
    {
        var emailIndex = Builders<User>.IndexKeys.Ascending(u => u.Email);
        await Users.Indexes.CreateOneAsync(new CreateIndexModel<User>(
            emailIndex,
            new CreateIndexOptions { Unique = true }));

        var orderNumberIndex = Builders<Order>.IndexKeys.Ascending(o => o.OrderNumber);
        await Orders.Indexes.CreateOneAsync(new CreateIndexModel<Order>(
            orderNumberIndex,
            new CreateIndexOptions { Unique = true }));

        var notificationOrderIdIndex = Builders<Notification>.IndexKeys.Ascending(n => n.OrderId);
        await Notifications.Indexes.CreateOneAsync(new CreateIndexModel<Notification>(
            notificationOrderIdIndex));

        var deliveryOrderIdIndex = Builders<Delivery>.IndexKeys.Ascending(d => d.OrderId);
        await Deliveries.Indexes.CreateOneAsync(new CreateIndexModel<Delivery>(
            deliveryOrderIdIndex));

        var refreshTokenHashIndex = Builders<RefreshToken>.IndexKeys.Ascending(t => t.TokenHash);
        await RefreshTokens.Indexes.CreateOneAsync(new CreateIndexModel<RefreshToken>(
            refreshTokenHashIndex,
            new CreateIndexOptions { Unique = true }));

        var refreshTokenExpiryIndex = Builders<RefreshToken>.IndexKeys.Ascending(t => t.ExpiresAt);
        await RefreshTokens.Indexes.CreateOneAsync(new CreateIndexModel<RefreshToken>(
            refreshTokenExpiryIndex,
            new CreateIndexOptions { ExpireAfter = TimeSpan.Zero }));
    }
}