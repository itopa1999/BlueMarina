using BlueMarina.Domain.Entities.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlueMarina.Infrastructure.Persistence.Configurations.Security;

public class SignInAttemptConfiguration : IEntityTypeConfiguration<SignInAttempt>
{
    public void Configure(EntityTypeBuilder<SignInAttempt> builder)
    {
        builder.Property(x => x.IpAddress).HasMaxLength(45);
        builder.Property(x => x.Location).HasMaxLength(255);

        builder.Property(x => x.Method).HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);

        // Note: UserId is long? in this entity, so we don't map it to the Identity User string PK.
        // If you change UserId to string later, uncomment the block below.
        
        // builder.HasOne<User>()
        //        .WithMany()
        //        .HasForeignKey(x => x.UserId)
        //        .OnDelete(DeleteBehavior.SetNull);
    }
}
