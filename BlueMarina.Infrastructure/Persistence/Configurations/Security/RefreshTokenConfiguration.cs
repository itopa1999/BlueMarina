using BlueMarina.Domain.Entities.Security;
using BlueMarina.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlueMarina.Infrastructure.Persistence.Configurations.Security;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        builder.Property(x => x.UserId).HasMaxLength(450).IsRequired();
        builder.Property(x => x.TokenHash).HasMaxLength(500).IsRequired();
        builder.Property(x => x.RevokedByIp).HasMaxLength(45);
        builder.Property(x => x.ReplacedByTokenHash).HasMaxLength(500);
        builder.Property(x => x.RevocationReason).HasMaxLength(255);
        builder.Property(x => x.CreatedByIp).HasMaxLength(45);
        builder.Property(x => x.DeviceName).HasMaxLength(255);
        builder.Property(x => x.UserAgent).HasMaxLength(500);

        // Ignore computed properties from EF mapping
        builder.Ignore(x => x.IsExpired);
        builder.Ignore(x => x.IsRevoked);
        builder.Ignore(x => x.IsActive);

        builder.HasIndex(x => x.TokenHash).IsUnique();

        builder.HasOne<User>()
               .WithMany()
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
