using BlueMarina.Application.Interfaces.Notification;
using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Application.Interfaces.Security;
using BlueMarina.Infrastructure.Authentication.Jwt;
using BlueMarina.Infrastructure.Identity;
using BlueMarina.Infrastructure.Notification;
using BlueMarina.Infrastructure.Persistence;
using BlueMarina.Infrastructure.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BlueMarina.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
    this IServiceCollection services,
    IConfiguration configuration)
    {
        services.AddIdentityServices(configuration);
        services.AddJwtAuthentication(configuration);
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IOtpService, OtpService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ISmsService, SmsService>();

        return services;
    }
}