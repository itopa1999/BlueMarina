using BlueMarina.Application.Interfaces.Caching;
using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Application.Interfaces.Security;
using BlueMarina.Infrastructure.Authentication.Jwt;
using BlueMarina.Infrastructure.Caching;
using BlueMarina.Infrastructure.Identity;
using BlueMarina.Infrastructure.Notification;
using BlueMarina.Infrastructure.Notification.Email;
using BlueMarina.Infrastructure.Persistence;
using BlueMarina.Infrastructure.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace BlueMarina.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
    this IServiceCollection services,
    IConfiguration configuration)
    {
        services.AddIdentityServices(configuration);
        services.AddJwtAuthentication(configuration);
        var redisConnectionString = configuration.GetConnectionString("Redis")
            ?? throw new InvalidOperationException("The Redis connection string is not configured.");
        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnectionString));
        services.AddScoped<ICacheService, RedisCacheService>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IOtpService, OtpService>();
        services.AddNotificationServices(configuration);
        services.AddSingleton<TemplateRenderer>();


        return services;
    }
}
