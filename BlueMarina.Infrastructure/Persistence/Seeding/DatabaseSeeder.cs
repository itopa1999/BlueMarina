using BlueMarina.Application.Interfaces.ExternalAPI;
using BlueMarina.Domain.Entities.Onboarding;
using BlueMarina.Infrastructure.Identity;
using BlueMarina.Shared.Logging;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BlueMarina.Infrastructure.Persistence.Seeding;

public sealed class DatabaseSeeder
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole<long>> _roleManager;
    private readonly IExternalApiClient _externalApiClient;
    private readonly ExternalApiSettings _settings;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(
        ApplicationDbContext dbContext,
        UserManager<User> userManager,
        RoleManager<IdentityRole<long>> roleManager,
        IExternalApiClient externalApiClient,
        IOptions<ExternalApiSettings> settings,
        ILogger<DatabaseSeeder> logger)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _roleManager = roleManager;
        _externalApiClient = externalApiClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedAgreementVersionsAsync(cancellationToken);
        await SeedLocationsAsync(cancellationToken);
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

    private async Task SeedLocationsAsync(CancellationToken cancellationToken)
    {
        using var op = OperationLogger.Start(_logger, "SeedLocations");

        if (await _dbContext.States.AnyAsync(cancellationToken))
        {
            op.Success("Nigerian States and LGAs already seeded.");
            return;
        }

        if (string.IsNullOrWhiteSpace(_settings.NigerianLocationsUrl))
        {
            op.Warning("NigerianLocationsUrl is not configured in appsettings/.env.");
            return;
        }

        try
        {
            var locations = await _externalApiClient.GetAsync<Dictionary<string, List<string>>>(
                _settings.NigerianLocationsUrl, 
                cancellationToken: cancellationToken);

            if (locations == null || !locations.Any())
            {
                op.Warning("Failed to fetch or deserialize location data from the external API.");
                return;
            }

            var statesToAdd = new List<State>();

            foreach (var stateGroup in locations)
            {
                var state = new State
                {
                    Name = stateGroup.Key,
                    Lgas = stateGroup.Value.Select(lgaName => new Lga
                    {
                        Name = lgaName
                    }).ToList()
                };

                statesToAdd.Add(state);
            }

            await _dbContext.States.AddRangeAsync(statesToAdd, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            op.Success($"Seeded {statesToAdd.Count} states and their LGAs.");
        }
        catch (Exception ex)
        {
            op.Error("Failed to seed locations.", ex);
        }
    }
}