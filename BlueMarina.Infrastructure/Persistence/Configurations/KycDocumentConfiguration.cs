using BlueMarina.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlueMarina.Infrastructure.Persistence.Configurations;

public sealed class KycDocumentConfiguration : IEntityTypeConfiguration<KycDocument>
{
    public void Configure(EntityTypeBuilder<KycDocument> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.UserId, x.Type })
               .IsUnique();

        builder.HasIndex(x => x.Status);

        builder.HasOne(x => x.UserProfile)
               .WithMany()
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
