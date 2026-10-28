using BlueMarina.Application.Common.Settings;
using BlueMarina.Application.Helpers;
using BlueMarina.Application.Interfaces.ExternalAPI;
using BlueMarina.Domain.Entities.Onboarding;
using BlueMarina.Domain.Enums;
using BlueMarina.Infrastructure.Identity;
using BlueMarina.Shared.Constants;
using BlueMarina.Shared.Logging;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
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
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(
        ApplicationDbContext dbContext,
        UserManager<User> userManager,
        RoleManager<IdentityRole<long>> roleManager,
        IExternalApiClient externalApiClient,
        IOptions<ExternalApiSettings> settings,
        IConfiguration configuration,
        IHostEnvironment environment,
        ILogger<DatabaseSeeder> logger)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _roleManager = roleManager;
        _externalApiClient = externalApiClient;
        _settings = settings.Value;
        _configuration = configuration;
        _environment = environment;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedAgreementVersionsAsync(cancellationToken);
        await SeedLocationsAsync(cancellationToken);
        await SeedTierLimitsAsync(cancellationToken);
        await SeedSuperAdminAsync(cancellationToken);
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

    private async Task SeedTierLimitsAsync(CancellationToken cancellationToken)
    {
        using var op = OperationLogger.Start(_logger, "SeedTierLimits");

        if (await _dbContext.TierLimits.AnyAsync(cancellationToken))
        {
            op.Success("Tier limits already seeded.");
            return;
        }

        var tierLimits = DefaultTierLimit.Create();
        
        await _dbContext.TierLimits.AddRangeAsync(tierLimits, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        op.Success($"Seeded {tierLimits.Count} tier limit records.");
    }

    private async Task SeedSuperAdminAsync(CancellationToken cancellationToken)
    {
        using var op = OperationLogger.Start(_logger, "SeedSuperAdmin");

        var superAdminRole = await _roleManager.FindByNameAsync(RolesConstants.SuperAdmin);
        
        if (superAdminRole is null)
        {
            superAdminRole = new IdentityRole<long> { Name = RolesConstants.SuperAdmin };
            await _roleManager.CreateAsync(superAdminRole);
        }

        var usersInRole = await _userManager.GetUsersInRoleAsync(RolesConstants.SuperAdmin);
        if (usersInRole.Any())
        {
            op.Success("SuperAdmin already exists. Skipping bootstrap.");
            return;
        }

        var email = _configuration["Seed:SuperAdmin:Email"];
        var phoneNumber = _configuration["Seed:SuperAdmin:PhoneNumber"];
        var password = _configuration["Seed:SuperAdmin:Password"];

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(phoneNumber) ||
            string.IsNullOrWhiteSpace(password))
        {
            if (_environment.IsProduction())
            {
                throw new InvalidOperationException(
                    "No SuperAdmin exists and seed credentials are not configured. " +
                    "Set Seed__SuperAdmin__Email, Seed__SuperAdmin__PhoneNumber, " +
                    "and Seed__SuperAdmin__Password in your .env file.");
            }

            op.Warning("No SuperAdmin exists and seed credentials are not configured. Skipping.");
            return;
        }

        var normalizedEmail = email.Trim().ToLowerInvariant();
        var normalizedPhone = ExtensionHelpers.Normalize(phoneNumber);

        if (normalizedPhone is null)
        {
            throw new InvalidOperationException(
                "Invalid phone number format in Seed__SuperAdmin__PhoneNumber.");
        }

        string publicId;
        var maxAttempts = 5;
        var attempt = 0;

        do
        {
            publicId = $"BM{Random.Shared.Next(100_000, 1_000_000)}";
            var exists = await _userManager.Users.AnyAsync(u => u.PublicId == publicId, cancellationToken);
            if (!exists) break;
            attempt++;
        } while (attempt < maxAttempts);

        if (attempt >= maxAttempts)
            throw new InvalidOperationException("Could not generate a unique PublicId for SuperAdmin.");

        var user = new User
        {
            Email = normalizedEmail,
            UserName = normalizedEmail,
            PhoneNumber = normalizedPhone,
            PublicId = publicId,
            Status = UserStatus.Active,
            EmailConfirmed = true,
            PhoneNumberConfirmed = true,
            Tier = AccountTier.Tier3
        };

        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to create SuperAdmin user: {errors}");
        }

        user.OwnReferralCode = $"{user.PublicId}{Random.Shared.Next(1000, 9999)}";
        await _userManager.UpdateAsync(user);

        var roleResult = await _userManager.AddToRoleAsync(user, RolesConstants.SuperAdmin);

        if (!roleResult.Succeeded)
        {
            var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to assign SuperAdmin role: {errors}");
        }

        op.Success($"SuperAdmin {publicId} created successfully. Please change the password immediately.");
    }
}