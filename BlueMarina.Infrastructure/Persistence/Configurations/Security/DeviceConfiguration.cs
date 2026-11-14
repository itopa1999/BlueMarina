using BlueMarina.Domain.Entities.Security;
using BlueMarina.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlueMarina.Infrastructure.Persistence.Configurations.Security;

public class DeviceConfiguration : IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> builder)
    {
        builder.Property(x => x.UserId).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Platform).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Model).HasMaxLength(100);
        builder.Property(x => x.PushToken).HasMaxLength(500);
        builder.Property(x => x.LastLocation).HasMaxLength(255);

        builder.HasOne<User>()
               .WithMany()
               .HasForeignKey(x => x.UserId)
.HasPrincipalKey(x => x.PublicId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

