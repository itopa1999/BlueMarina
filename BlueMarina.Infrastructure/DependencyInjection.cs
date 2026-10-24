using BlueMarina.Application.Common.Settings;
using BlueMarina.Application.Interfaces.Caching;
using BlueMarina.Application.Interfaces.ExternalAPI;
using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Application.Interfaces.Security;
using BlueMarina.Infrastructure.Authentication.Jwt;
using BlueMarina.Infrastructure.Caching;
using BlueMarina.Infrastructure.ExternalAPI;
using BlueMarina.Infrastructure.Identity;
using BlueMarina.Infrastructure.Notification;
using BlueMarina.Infrastructure.Notification.Email;
using BlueMarina.Infrastructure.Persistence;
using BlueMarina.Infrastructure.Persistence.Seeding;
using BlueMarina.Infrastructure.Security;
using BlueMarina.Infrastructure.Service;
using Hangfire;
using Hangfire.PostgreSql;
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

        var postgresConnectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Postgres connection string is missing.");

        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(
                options => options.UseNpgsqlConnection(postgresConnectionString),
                new PostgreSqlStorageOptions()));

        services.AddHangfireServer();

        var redisConnectionString = configuration["Redis:URL"]
            ?? throw new InvalidOperationException("Redis connection string is missing.");

        var redisOptions = ConfigurationOptions.Parse(redisConnectionString);

        redisOptions.AbortOnConnectFail = false;
        redisOptions.ConnectTimeout = 1000;
        redisOptions.SyncTimeout = 1000;
        redisOptions.AsyncTimeout = 1000;
        redisOptions.ConnectRetry = 1;
        redisOptions.ClientName = "BLUEMARINA-API";

        services.AddSingleton<IConnectionMultiplexer>(
            _ => ConnectionMultiplexer.Connect(redisOptions));
        services.AddScoped<ICacheService, RedisCacheService>();

        services.AddHttpContextAccessor();

        services.Configure<KycFileSettings>(
            configuration.GetSection(KycFileSettings.SectionName));

        var hmacKey = configuration["Security:HmacKey"]
            ?? throw new InvalidOperationException("HMAC Key is missing.");

        services.AddSingleton(new SensitiveIdentifierHasher(hmacKey));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IOtpService, OtpService>();

        services.AddNotificationServices(configuration);
        services.AddSingleton<TemplateRenderer>();

        services.Configure<ExternalApiSettings>(
            configuration.GetSection(ExternalApiSettings.SectionName));

        services.AddHttpClient<IExternalApiClient, ExternalApiClient>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddScoped<DatabaseSeeder>();

        return services;
    }
}