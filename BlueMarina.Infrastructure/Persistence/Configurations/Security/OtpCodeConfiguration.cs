using BlueMarina.Domain.Entities.Security;
using BlueMarina.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlueMarina.Infrastructure.Persistence.Configurations.Security;

public class OtpCodeConfiguration : IEntityTypeConfiguration<OtpCode>
{
    public void Configure(EntityTypeBuilder<OtpCode> builder)
    {
        builder.ToTable("otp_codes");

        builder.Property(x => x.UserId).HasMaxLength(100).IsRequired();
        builder.Property(x => x.CodeHash).HasMaxLength(500).IsRequired();

        builder.Property(x => x.Channel).HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.Purpose).HasConversion<string>().HasMaxLength(100);

        builder.HasIndex(x => x.UserId);

        builder.HasOne<User>()
               .WithMany()
               .HasForeignKey(x => x.UserId)
.HasPrincipalKey(x => x.PublicId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

