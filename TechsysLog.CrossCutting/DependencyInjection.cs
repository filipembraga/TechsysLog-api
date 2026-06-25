using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Techsyslog.Infrastructure.Settings;
using TechsysLog.Infrastructure.Context;
using TechsysLog.Infrastructure.Repositories;
using TechsysLog.Domain.Interfaces;
using TechsysLog.Application.Interfaces;
using TechsysLog.Application.Services;
using TechsysLog.Application.Settings;
using TechsysLog.Infrastructure.ExternalServices;
using TechsysLog.Infrastructure.WebSockets;
using MongoDB.Driver;

namespace TechsysLog.CrossCutting;

/// <summary>
/// Centralized Dependency Injection configuration for the entire solution.
/// 
/// This class is the only place in the solution aware of all concrete
/// implementations, keeping the API layer decoupled from infrastructure details.
/// </summary>
[ExcludeFromCodeCoverage]
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<MongoDbSettings>(configuration.GetSection("MongoDb"));
        services.AddSingleton<MongoDbContext>();
        services.AddSingleton<IMongoClient>(sp =>
            sp.GetRequiredService<MongoDbContext>().Client);
        services.AddSignalR();

        services.AddViaCepClient(configuration);

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IDeliveryRepository, DeliveryRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<INotificationDispatcher, SignalRNotificationDispatcher>();

        return services;
    }

    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.AddScoped<ITokenService, TokenService>();

        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IDeliveryService, DeliveryService>();
        services.AddScoped<INotificationService, NotificationService>();

        return services;
    }
}