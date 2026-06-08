using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Techsyslog.Infrastructure.Settings;
using TechsysLog.Infrastructure.Context;
using TechsysLog.Infrastructure.Repositories;
using TechsysLog.Domain.Interfaces;

namespace TechsysLog.CrossCutting;

/// <summary>
/// Centralized Dependency Injection configuration for the entire solution.
/// 
/// This class is the only place in the solution aware of all concrete
/// implementations, keeping the API layer decoupled from infrastructure details.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    { 
        services.Configure<MongoDbSettings>(configuration.GetSection("MongoDb"));
        services.AddSingleton<MongoDbContext>();
        
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IDeliveryRepository, DeliveryRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        
        return services;
    }

    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    { 
        ///services.AddScoped<IUserService, UserService>();
        ///services.AddScoped<IOrderService, OrderService>();
        ///services.AddScoped<IDeliveryService, DeliveryService>();
        ///services.AddScoped<INotificationService, NotificationService>();
        return services;
     }
}