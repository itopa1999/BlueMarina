using BlueMarina.Domain.Entities.Security;
using BlueMarina.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlueMarina.Infrastructure.Persistence.Configurations.Security;

public class TwoFactorMethodConfiguration : IEntityTypeConfiguration<TwoFactorMethod>
{
    public void Configure(EntityTypeBuilder<TwoFactorMethod> builder)
    {
        builder.Property(x => x.UserId).HasMaxLength(450).IsRequired();
        builder.Property(x => x.SecretEncrypted).HasMaxLength(500);

        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(50);

        builder.HasOne<User>()
               .WithMany()
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
