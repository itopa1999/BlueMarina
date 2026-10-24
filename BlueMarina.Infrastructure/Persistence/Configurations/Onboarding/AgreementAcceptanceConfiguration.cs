using BlueMarina.Domain.Entities.Onboarding;
using BlueMarina.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlueMarina.Infrastructure.Persistence.Configurations.Onboarding;

public class AgreementAcceptanceConfiguration : IEntityTypeConfiguration<AgreementAcceptance>
{
    public void Configure(EntityTypeBuilder<AgreementAcceptance> builder)
    {
        builder.Property(x => x.UserId).HasMaxLength(450).IsRequired();
        builder.Property(x => x.IpAddress).HasMaxLength(45);

        // AgreementVersion PK is long (from BaseEntity), so FK must be long
        builder.HasOne(x => x.AgreementVersion)
               .WithMany()
               .HasForeignKey(x => x.AgreementVersionId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
               .WithMany()
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}