using BlueMarina.Domain.Entities.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlueMarina.Infrastructure.Persistence.Configurations.Security;

public class OtpLockoutConfiguration : IEntityTypeConfiguration<OtpLockout>
{
    public void Configure(EntityTypeBuilder<OtpLockout> builder)
    {
        builder.Property(x => x.UserId).HasMaxLength(100).IsRequired();

        builder.Property(x => x.Purpose).HasConversion<string>().HasMaxLength(100);
        builder.Property(x => x.Channel).HasConversion<string>().HasMaxLength(50);

        builder.HasIndex(x => new { x.UserId, x.Purpose, x.Channel }).IsUnique();
    }
}