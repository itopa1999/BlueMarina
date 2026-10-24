using BlueMarina.Domain.Entities.Onboarding;
using BlueMarina.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlueMarina.Infrastructure.Persistence.Configurations.Onboarding;

public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.Property(x => x.UserId).HasMaxLength(450).IsRequired();
        builder.Property(x => x.Street).HasMaxLength(255);
        builder.Property(x => x.Area).HasMaxLength(255);
        builder.Property(x => x.State).HasMaxLength(100);
        builder.Property(x => x.Lga).HasMaxLength(100);
        builder.Property(x => x.ProofDocumentUrl).HasMaxLength(500);

        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);

        builder.HasOne<User>()
               .WithMany()
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
