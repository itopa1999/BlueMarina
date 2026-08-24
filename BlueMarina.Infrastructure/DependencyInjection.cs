using BlueMarina.Application.Common.Settings;
using BlueMarina.Application.Interfaces.Caching;
using BlueMarina.Application.Interfaces.Kyc;
using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Application.Interfaces.Security;
using BlueMarina.Application.Interfaces.Storage;
using BlueMarina.Infrastructure.Authentication.Jwt;
using BlueMarina.Infrastructure.Caching;
using BlueMarina.Infrastructure.Identity;
using BlueMarina.Infrastructure.Kyc;
using BlueMarina.Infrastructure.Notification;
using BlueMarina.Infrastructure.Notification.Email;
using BlueMarina.Infrastructure.Persistence;
using BlueMarina.Infrastructure.Security;
using BlueMarina.Infrastructure.Storage;
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
        services.Configure<LocalStorageSettings>(
            configuration.GetSection(LocalStorageSettings.SectionName));
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.Configure<KycFileSettings>(
            configuration.GetSection(KycFileSettings.SectionName));

        services.AddScoped<ICacheService, RedisCacheService>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IOtpService, OtpService>();
        services.AddScoped<IKycService, KycService>();
        services.AddNotificationServices(configuration);
        services.AddSingleton<TemplateRenderer>();

        // services.Configure<S3StorageSettings>(
        //     configuration.GetSection(
        //         S3StorageSettings.SectionName));

        // var s3Settings =
        //     configuration
        //         .GetSection(S3StorageSettings.SectionName)
        //         .Get<S3StorageSettings>()
        //     ?? throw new InvalidOperationException(
        //         "S3 storage configuration is missing.");

        // if (string.IsNullOrWhiteSpace(s3Settings.BucketName))
        //     throw new InvalidOperationException(
        //         "S3 bucket name is missing.");

        // if (string.IsNullOrWhiteSpace(s3Settings.Region))
        //     throw new InvalidOperationException(
        //         "S3 region is missing.");

        // var credentials = new BasicAWSCredentials(
        //     s3Settings.AccessKey,
        //     s3Settings.SecretKey);

        // var awsRegion =
        //     RegionEndpoint.GetBySystemName(
        //         s3Settings.Region);

        // services.AddSingleton<IAmazonS3>(
        //     new AmazonS3Client(
        //         credentials,
        //         awsRegion));

        // services.AddScoped<
        //     IFileStorageService,
        //     S3FileStorageService>();


        return services;
    }
}
