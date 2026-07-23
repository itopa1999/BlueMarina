using BlueMarina.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlueMarina.Infrastructure.Persistence.Configurations;

public sealed class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.Bvn)
               .IsUnique();

        builder.HasIndex(x => x.Nin)
               .IsUnique();

        builder.HasIndex(x => x.ReferralCode)
               .IsUnique();

        builder.HasIndex(x => x.KycLevel);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}