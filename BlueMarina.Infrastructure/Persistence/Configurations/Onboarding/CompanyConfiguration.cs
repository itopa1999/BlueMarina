using BlueMarina.Domain.Entities.Onboarding;
using BlueMarina.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlueMarina.Infrastructure.Persistence.Configurations.Onboarding;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.Property(x => x.Name).HasMaxLength(255).IsRequired();
        builder.Property(x => x.RcNumber).HasMaxLength(100).IsRequired();
        builder.Property(x => x.NatureOfBusiness).HasMaxLength(500);
        builder.Property(x => x.Tin).HasMaxLength(50);
        builder.Property(x => x.RegisteredAddress).HasMaxLength(500);

        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.CreatedByUserId).HasMaxLength(100).IsRequired();
        
        builder.HasIndex(x => x.RcNumber).IsUnique();

        builder.HasOne<User>()
               .WithMany()
               .HasForeignKey(x => x.CreatedByUserId)
.HasPrincipalKey(x => x.PublicId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

