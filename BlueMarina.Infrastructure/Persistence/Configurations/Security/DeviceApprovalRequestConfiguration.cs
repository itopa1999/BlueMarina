using BlueMarina.Domain.Entities.Security;
using BlueMarina.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlueMarina.Infrastructure.Persistence.Configurations.Security;

public class DeviceApprovalRequestConfiguration : IEntityTypeConfiguration<DeviceApprovalRequest>
{
    public void Configure(EntityTypeBuilder<DeviceApprovalRequest> builder)
    {
        builder.Property(x => x.UserId).HasMaxLength(450).IsRequired();

        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);

        builder.HasOne<User>()
               .WithMany()
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);
               
        // Optional: If Device uses Guid for its PK, you can map these too.
        // Assuming Device Id is a Guid (or long mapped via conversion), uncomment below if needed:
        // builder.HasOne<Device>().WithMany().HasForeignKey(x => x.NewDeviceId).OnDelete(DeleteBehavior.Restrict);
        // builder.HasOne<Device>().WithMany().HasForeignKey(x => x.ApproverDeviceId).OnDelete(DeleteBehavior.Restrict);
    }
}
