using BlueMarina.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlueMarina.Infrastructure.Persistence.Configurations;

public sealed class EmploymentInformationConfiguration : IEntityTypeConfiguration<EmploymentInformation>
{
    public void Configure(EntityTypeBuilder<EmploymentInformation> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.UserId).IsUnique();

        builder.HasOne(x => x.UserProfile)
            .WithOne()
            .HasForeignKey<EmploymentInformation>(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
