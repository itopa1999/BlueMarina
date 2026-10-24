using BlueMarina.Domain.Entities.Onboarding;
using BlueMarina.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlueMarina.Infrastructure.Persistence.Configurations.Onboarding;

public class CompanyMemberConfiguration : IEntityTypeConfiguration<CompanyMember>
{
    public void Configure(EntityTypeBuilder<CompanyMember> builder)
    {
        builder.Property(x => x.UserId).HasMaxLength(450);
        builder.Property(x => x.FullName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(255).IsRequired();
        builder.Property(x => x.Phone).HasMaxLength(20).IsRequired();
        
        builder.Property(x => x.OwnershipPercent).HasPrecision(5, 2);

        builder.Property(x => x.MemberType).HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.Roles).HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.SigningMandate).HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.VerificationStatus).HasConversion<string>().HasMaxLength(50);

        builder.HasOne(x => x.Company)
               .WithMany(x => x.Members)
               .HasForeignKey(x => x.CompanyId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>()
               .WithMany()
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
