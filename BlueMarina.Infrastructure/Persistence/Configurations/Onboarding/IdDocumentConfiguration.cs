using BlueMarina.Domain.Entities.Onboarding;
using BlueMarina.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlueMarina.Infrastructure.Persistence.Configurations.Onboarding;

public class IdDocumentConfiguration : IEntityTypeConfiguration<IdDocument>
{
    public void Configure(EntityTypeBuilder<IdDocument> builder)
    {
        builder.Property(x => x.UserId).HasMaxLength(100).IsRequired();
        builder.Property(x => x.IdNumber).HasMaxLength(100).IsRequired();
        builder.Property(x => x.NameOnId).HasMaxLength(255).IsRequired();
        builder.Property(x => x.ImageUrl).HasMaxLength(500).IsRequired();
        builder.Property(x => x.RejectionReason).HasMaxLength(500);

        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);

        builder.HasOne<User>()
               .WithMany()
               .HasForeignKey(x => x.UserId)
.HasPrincipalKey(x => x.PublicId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

