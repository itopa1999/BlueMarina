using BlueMarina.Domain.Entities.Onboarding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlueMarina.Infrastructure.Persistence.Configurations.Onboarding;

public class CompanyInvitationConfiguration : IEntityTypeConfiguration<CompanyInvitation>
{
    public void Configure(EntityTypeBuilder<CompanyInvitation> builder)
    {
        builder.Property(x => x.CompanyMemberUserId).HasMaxLength(100).IsRequired();
        builder.Property(x => x.TokenHash).HasMaxLength(255).IsRequired();
        builder.Property(x => x.AcceptedWithEmail).HasMaxLength(255);

        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);
        
        builder.HasIndex(x => x.TokenHash).IsUnique();

        builder.HasOne<Company>()
               .WithMany()
               .HasForeignKey(x => x.CompanyId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

