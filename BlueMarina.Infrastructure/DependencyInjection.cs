using BlueMarina.Infrastructure.Authentication.Jwt;
using BlueMarina.Infrastructure.Identity;
using BlueMarina.Infrastructure.Persistence;
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

        return services;
    }
}