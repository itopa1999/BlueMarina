using BlueMarina.Domain.Entities.Onboarding;
using BlueMarina.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlueMarina.Infrastructure.Persistence.Configurations.Onboarding;

public class UserOnboardingProgressConfiguration : IEntityTypeConfiguration<UserOnboardingProgress>
{
    public void Configure(EntityTypeBuilder<UserOnboardingProgress> builder)
    {
        builder.ToTable("user_onboarding_progress");

        builder.HasIndex(x => x.UserId).IsUnique();

        builder.HasOne<User>()
               .WithMany()
               .HasForeignKey(x => x.UserId)
                .HasPrincipalKey(x => x.PublicId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}