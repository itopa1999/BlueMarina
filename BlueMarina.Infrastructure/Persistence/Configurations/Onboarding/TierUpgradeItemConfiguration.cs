using BlueMarina.Domain.Entities.Onboarding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlueMarina.Infrastructure.Persistence.Configurations.Onboarding;

public class TierUpgradeItemConfiguration : IEntityTypeConfiguration<TierUpgradeItem>
{
    public void Configure(EntityTypeBuilder<TierUpgradeItem> builder)
    {
        builder.Property(x => x.Reason).HasMaxLength(500);
        
        builder.Property(x => x.ItemType).HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);
    }
}

