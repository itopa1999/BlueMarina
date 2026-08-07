using BlueMarina.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlueMarina.Infrastructure.Persistence.Configurations;

public sealed class KycProfileConfiguration : IEntityTypeConfiguration<KycProfile>
{
    public void Configure(EntityTypeBuilder<KycProfile> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.UserId).IsUnique();
        builder.HasIndex(x => x.Bvn).IsUnique();
        builder.HasIndex(x => x.Nin).IsUnique();
        builder.HasIndex(x => x.KycLevel);

        builder.HasOne(x => x.UserProfile)
            .WithOne()
            .HasForeignKey<KycProfile>(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
