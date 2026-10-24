using BlueMarina.Domain.Entities.Onboarding;
using BlueMarina.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlueMarina.Infrastructure.Persistence.Configurations.Onboarding;

public class TaxResidencyConfiguration : IEntityTypeConfiguration<TaxResidency>
{
    public void Configure(EntityTypeBuilder<TaxResidency> builder)
    {
        builder.Property(x => x.UserId).HasMaxLength(450).IsRequired();
        builder.Property(x => x.CountryCode).HasMaxLength(2).IsRequired();
        builder.Property(x => x.Tin).HasMaxLength(50);
        
        builder.Property(x => x.NoTinReason).HasConversion<string>().HasMaxLength(100);

        builder.HasOne<User>()
               .WithMany()
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
