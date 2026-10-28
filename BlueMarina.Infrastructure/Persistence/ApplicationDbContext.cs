using System.Linq.Expressions;
using BlueMarina.Domain.Common;
using BlueMarina.Domain.Entities.Security;
using BlueMarina.Domain.Entities.Onboarding; 
using BlueMarina.Infrastructure.Identity;
using BlueMarina.Infrastructure.Service;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BlueMarina.Domain.Entities.Home;

namespace BlueMarina.Infrastructure.Persistence;

public sealed class ApplicationDbContext
    : IdentityDbContext<User, IdentityRole<long>, long>
{
    private readonly ICurrentUserService? _currentUser;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService? currentUser)
        : base(options)
    {
        _currentUser = currentUser;
    }

    // --- Security Entities ---
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<OtpCode> OtpCodes => Set<OtpCode>();
    public DbSet<OtpLockout> OtpLockouts => Set<OtpLockout>();
    public DbSet<Credential> Credentials => Set<Credential>();
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<DeviceApprovalRequest> DeviceApprovalRequests => Set<DeviceApprovalRequest>();
    public DbSet<SignInAttempt> SignInAttempts => Set<SignInAttempt>();
    public DbSet<TwoFactorMethod> TwoFactorMethods => Set<TwoFactorMethod>();
    public DbSet<PinChallenge> PinChallenges => Set<PinChallenge>();

    // --- Onboarding Entities ---
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<AgreementAcceptance> AgreementAcceptances => Set<AgreementAcceptance>();
    public DbSet<AgreementVersion> AgreementVersions => Set<AgreementVersion>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<CompanyDocument> CompanyDocuments => Set<CompanyDocument>();
    public DbSet<CompanyInvitation> CompanyInvitations => Set<CompanyInvitation>();
    public DbSet<CompanyMember> CompanyMembers => Set<CompanyMember>();
    public DbSet<Consent> Consents => Set<Consent>();
    public DbSet<IdDocument> IdDocuments => Set<IdDocument>();
    public DbSet<IdentityCheck> IdentityChecks => Set<IdentityCheck>();
    public DbSet<NextOfKin> NextOfKins => Set<NextOfKin>();
    public DbSet<SelfieCheck> SelfieChecks => Set<SelfieCheck>();
    public DbSet<Signature> Signatures => Set<Signature>();
    public DbSet<TaxResidency> TaxResidencies => Set<TaxResidency>();
    public DbSet<TierLimit> TierLimits => Set<TierLimit>();
    public DbSet<TierUpgradeItem> TierUpgradeItems => Set<TierUpgradeItem>();
    public DbSet<TierUpgradeRequest> TierUpgradeRequests => Set<TierUpgradeRequest>();
    public DbSet<UserOnboardingProgress> UserOnboardingProgresses => Set<UserOnboardingProgress>();
    public DbSet<State> States => Set<State>();
    public DbSet<Lga> Lgas => Set<Lga>();

    // --- Home Entities ---
    public DbSet<Bank> Banks => Set<Bank>();
    public DbSet<BankAccount> BankAccounts => Set<BankAccount>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                continue;

            if (entityType.IsOwned())
                continue;

            var parameter = Expression.Parameter(entityType.ClrType, "e");

            var isDeletedProperty = Expression.Property(
                parameter,
                nameof(BaseEntity.IsDeleted));

            var filter = Expression.Lambda(
                Expression.Equal(
                    isDeletedProperty,
                    Expression.Constant(false)),
                parameter);

            modelBuilder
                .Entity(entityType.ClrType)
                .HasQueryFilter(filter);
        }
    }

    public override async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTimeOffset.UtcNow;
                    break;

                case EntityState.Modified:
                    entry.Entity.ModifiedAt = DateTimeOffset.UtcNow;
                    entry.Entity.ModifiedBy = _currentUser?.UserEmail?.ToString();
                    break;

                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = DateTimeOffset.UtcNow;
                    entry.Entity.DeletedBy = _currentUser?.UserEmail?.ToString();
                    break;
            }
        }

        foreach (var entry in ChangeTracker.Entries()
                     .Where(x => x.State is EntityState.Added or EntityState.Modified))
        {
            foreach (var property in entry.Properties)
            {
                if (property.CurrentValue is DateTimeOffset value &&
                    value.Offset != TimeSpan.Zero)
                {
                    property.CurrentValue = value.ToUniversalTime();
                }
            }
        }

        return await base.SaveChangesAsync(
            cancellationToken);
    }
}