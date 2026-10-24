using BlueMarina.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlueMarina.Infrastructure.Persistence.Configurations.Identity;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Configure PublicId as a Unique Alternate Key
        builder.HasIndex(x => x.PublicId).IsUnique();
        builder.HasAlternateKey(x => x.PublicId);
    }
}
