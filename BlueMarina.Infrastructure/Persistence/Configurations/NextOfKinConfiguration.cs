using BlueMarina.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlueMarina.Infrastructure.Persistence.Configurations;

public sealed class NextOfKinConfiguration : IEntityTypeConfiguration<NextOfKin>
{
    public void Configure(EntityTypeBuilder<NextOfKin> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.UserId).IsUnique();

        builder.HasOne(x => x.UserProfile)
            .WithOne()
            .HasForeignKey<NextOfKin>(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
