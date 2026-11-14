using BlueMarina.Domain.Entities.Onboarding;
using BlueMarina.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlueMarina.Infrastructure.Persistence.Configurations.Onboarding;

public class SelfieCheckConfiguration : IEntityTypeConfiguration<SelfieCheck>
{
    public void Configure(EntityTypeBuilder<SelfieCheck> builder)
    {
        builder.Property(x => x.UserId).HasMaxLength(100).IsRequired();
        builder.Property(x => x.MediaUrl).HasMaxLength(500);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);

        builder.HasOne<User>()
               .WithMany()
               .HasForeignKey(x => x.UserId)
.HasPrincipalKey(x => x.PublicId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

