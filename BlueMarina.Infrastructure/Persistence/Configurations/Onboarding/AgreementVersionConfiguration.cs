using BlueMarina.Domain.Entities.Onboarding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlueMarina.Infrastructure.Persistence.Configurations.Onboarding;

public class AgreementVersionConfiguration : IEntityTypeConfiguration<AgreementVersion>
{
    public void Configure(EntityTypeBuilder<AgreementVersion> builder)
    {
        builder.Property(x => x.Version).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ContentUrl).HasMaxLength(500).IsRequired();
        
        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(50);

        builder.HasIndex(x => new { x.Type, x.Version }).IsUnique();
    }
}

