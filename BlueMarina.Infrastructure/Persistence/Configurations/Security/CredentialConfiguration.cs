using BlueMarina.Domain.Entities.Security;
using BlueMarina.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlueMarina.Infrastructure.Persistence.Configurations.Security;

public class CredentialConfiguration : IEntityTypeConfiguration<Credential>
{
    public void Configure(EntityTypeBuilder<Credential> builder)
    {
        builder.Property(x => x.UserId).HasMaxLength(450).IsRequired();
        builder.Property(x => x.LoginPinHash).HasMaxLength(500);
        builder.Property(x => x.TransactionPinHash).HasMaxLength(500);

        // Only one credential record per user
        builder.HasIndex(x => x.UserId).IsUnique();

        builder.HasOne<User>()
               .WithMany()
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
