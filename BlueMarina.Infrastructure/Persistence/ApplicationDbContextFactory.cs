
using BlueMarina.Infrastructure.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BlueMarina.Infrastructure.Persistence;

public sealed class ApplicationDbContextFactory
    : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__Postgres")
            ?? "Host=localhost;Port=5432;Database=bluemarina_design;Username=bluemarina_design;Password=design_only";

        var optionsBuilder =
            new DbContextOptionsBuilder<ApplicationDbContext>();

        optionsBuilder.UseNpgsql(connectionString);

        return new ApplicationDbContext(
            optionsBuilder.Options,
            new DesignTimeCurrentUserService());
    }

    private sealed class DesignTimeCurrentUserService : ICurrentUserService
    {
        // Implement the members required by your ICurrentUserService interface.
        // These values are only for design-time migrations.
        public string? UserEmail => null;
    }
}
