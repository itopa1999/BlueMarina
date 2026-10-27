using BlueMarina.Shared.Constants;
using Microsoft.AspNetCore.Identity;

namespace BlueMarina.Infrastructure.Identity;

public static class IdentitySeeder
{
    public static async Task SeedRolesAsync(
        RoleManager<IdentityRole<long>> roleManager)
    {
        var roles = new[]
        {
            RolesConstants.Customer,
            RolesConstants.Admin,
            RolesConstants.SuperAdmin,
            RolesConstants.ComplianceOfficer
        };

        foreach (var role in roles)
        {
            if (await roleManager.RoleExistsAsync(role))
            {
                continue;
            }

            await roleManager.CreateAsync(
                new IdentityRole<long>
                {
                    Name = role
                });
        }
    }
}