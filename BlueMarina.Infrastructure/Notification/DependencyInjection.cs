using BlueMarina.Application.Interfaces.Notification;
using BlueMarina.Infrastructure.Jobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BlueMarina.Infrastructure.Notification;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<EmailSettings>(
            configuration.GetSection(EmailSettings.SectionName));

        services.AddScoped<IEmailService, EmailService>();

        services.AddScoped<ISmsService, SmsService>();
        
        services.AddScoped<INotificationQueue, NotificationQueue>();

        return services;
    }
}