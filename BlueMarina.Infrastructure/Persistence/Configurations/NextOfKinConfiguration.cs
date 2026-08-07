using BlueMarina.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlueMarina.Infrastructure.Persistence.Configurations;

public sealed class NextOfKinConfiguration : IEntityTypeConfiguration<NextOfKin>
{
    public void Configure(EntityTypeBuilder<NextOfKin> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.UserProfileId).IsUnique();

        builder.HasOne(x => x.UserProfile)
            .WithOne()
            .HasForeignKey<NextOfKin>(x => x.UserProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
