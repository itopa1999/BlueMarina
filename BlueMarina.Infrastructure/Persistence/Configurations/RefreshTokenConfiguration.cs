using BlueMarina.Domain.Entities;
using BlueMarina.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlueMarina.Infrastructure.Persistence.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TokenHash)
               .IsRequired()
               .HasMaxLength(256);

        builder.HasIndex(x => x.TokenHash)
               .IsUnique();

        builder.HasIndex(x => x.UserId);

        builder.HasIndex(x => x.ExpiresAt);

        builder.HasOne<User>()
               .WithMany(x => x.RefreshTokens)
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}