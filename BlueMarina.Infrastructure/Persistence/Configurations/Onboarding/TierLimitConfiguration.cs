using BlueMarina.Domain.Entities.Onboarding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlueMarina.Infrastructure.Persistence.Configurations.Onboarding;

public class TierLimitConfiguration : IEntityTypeConfiguration<TierLimit>
{
    public void Configure(EntityTypeBuilder<TierLimit> builder)
    {
        builder.Property(x => x.Tier).HasConversion<string>().HasMaxLength(50);
        
        builder.HasIndex(x => x.Tier).IsUnique();
    }
}

