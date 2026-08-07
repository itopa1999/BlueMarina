using BlueMarina.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlueMarina.Infrastructure.Persistence.Configurations;

public sealed class IncomeProfileConfiguration : IEntityTypeConfiguration<IncomeProfile>
{
    public void Configure(EntityTypeBuilder<IncomeProfile> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.UserProfileId).IsUnique();

        builder.HasOne(x => x.UserProfile)
            .WithOne()
            .HasForeignKey<IncomeProfile>(x => x.UserProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
