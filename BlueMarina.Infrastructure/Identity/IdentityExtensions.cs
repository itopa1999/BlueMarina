using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace BlueMarina.Infrastructure.Identity;

public static class IdentityExtensions
{
    public static async Task SeedIdentityAsync(
        this IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var roleManager = scope.ServiceProvider
            .GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        await IdentitySeeder.SeedRolesAsync(roleManager);
    }
}