using BlueMarina.Domain.Entities.Onboarding;
using BlueMarina.Infrastructure.Identity;
using BlueMarina.Shared.Logging;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlueMarina.Infrastructure.Persistence.Seeding;

public sealed class DatabaseSeeder
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole<long>> _roleManager;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(
        ApplicationDbContext dbContext,
        UserManager<User> userManager,
        RoleManager<IdentityRole<long>> roleManager,
        ILogger<DatabaseSeeder> logger)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task SeedAsync(
        CancellationToken cancellationToken = default)
    {
        await SeedAgreementVersionsAsync(cancellationToken);
    }

    private async Task SeedAgreementVersionsAsync(CancellationToken cancellationToken)
    {
        using var op = OperationLogger.Start(_logger, "SeedAgreementVersions");

        if (await _dbContext.AgreementVersions.AnyAsync(cancellationToken))
        {
            op.Success("Agreement versions already seeded.");
            return;
        }

        var versions = DefaultAgreementVersion.Create();
        
        await _dbContext.AgreementVersions.AddRangeAsync(versions, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        op.Success($"Seeded {versions.Count} agreement versions.");
    }
}