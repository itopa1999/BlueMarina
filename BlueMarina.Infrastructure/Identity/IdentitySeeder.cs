using BlueMarina.Shared.Constants;
using Microsoft.AspNetCore.Identity;

namespace BlueMarina.Infrastructure.Identity;

public static class IdentitySeeder
{
    public static async Task SeedRolesAsync(
        RoleManager<IdentityRole<Guid>> roleManager)
    {
        var roles = new[]
        {
            Roles.Customer,
            Roles.Admin,
            Roles.SuperAdmin,
            Roles.ComplianceOfficer
        };

        foreach (var role in roles)
        {
            if (await roleManager.RoleExistsAsync(role))
            {
                continue;
            }

            await roleManager.CreateAsync(
                new IdentityRole<Guid>
                {
                    Name = role
                });
        }
    }
}