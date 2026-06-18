using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using TechsysLog.Domain.Interfaces;

namespace TechsysLog.Infrastructure.ExternalServices;

public static class ViaCepClientExtensions
{
    public static IServiceCollection AddViaCepClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpClient<IAddressLookupService, ViaCepClient>(client =>
        {
            client.BaseAddress = new Uri(configuration["ViaCep:BaseUrl"]
                ?? "https://viacep.com.br/ws/");
        })
        .AddResilienceHandler("viacep", pipeline =>
        {
            pipeline.AddRetry(new HttpRetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromMilliseconds(200),
                BackoffType = DelayBackoffType.Exponential,
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .Handle<HttpRequestException>()
                    .HandleResult(r => (int)r.StatusCode >= 500)
            });

            pipeline.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
            {
                SamplingDuration = TimeSpan.FromSeconds(30),
                FailureRatio = 0.5,
                MinimumThroughput = 5,
                BreakDuration = TimeSpan.FromSeconds(15)
            });

            pipeline.AddTimeout(TimeSpan.FromSeconds(3));
        });

        return services;
    }
}