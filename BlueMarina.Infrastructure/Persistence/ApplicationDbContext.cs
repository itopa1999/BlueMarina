using System.Linq.Expressions;
using BlueMarina.Domain.Common;
using BlueMarina.Domain.Entities;
using BlueMarina.Infrastructure.Identity;
using BlueMarina.Infrastructure.Service;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

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

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<OtpVerification> OtpVerifications => Set<OtpVerification>();

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
                    entry.Entity.ModifiedBy = _currentUser?.UserEmail.ToString();
                    break;

                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = DateTimeOffset.UtcNow;
                    entry.Entity.DeletedBy = _currentUser?.UserEmail.ToString();
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
