using BlueMarina.Domain.Entities.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace BlueMarina.Infrastructure.Persistence.Configurations.Security;

public sealed class PinChallengeConfiguration : IEntityTypeConfiguration<PinChallenge>
{
    public void Configure(EntityTypeBuilder<PinChallenge> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PinChallengeType)
               .IsRequired()
               .HasMaxLength(256);

        builder.HasIndex(c => new
        {
            c.UserId,
            c.PinChallengeType,
            c.IsUsed,
            c.ExpiresAt
        });
    }
}