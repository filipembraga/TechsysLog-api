using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Driver;

namespace TechsysLog.Infrastructure.HealthChecks;

public class MongoDbHealthCheck : IHealthCheck
{
    private readonly IMongoClient _client;

    public MongoDbHealthCheck(IMongoClient client)
    {
        _client = client;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _client.ListDatabaseNamesAsync(cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(ex.Message);
        }
    }
}