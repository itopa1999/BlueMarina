using BlueMarina.Domain.Entities.Onboarding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlueMarina.Infrastructure.Persistence.Configurations.Onboarding;

public class StateConfiguration : IEntityTypeConfiguration<State>
{
    public void Configure(EntityTypeBuilder<State> builder)
    {
        builder.ToTable("states");
        
        builder.HasIndex(x => x.Name).IsUnique();
        
        builder.HasMany(x => x.Lgas)
               .WithOne(x => x.State)
               .HasForeignKey(x => x.StateId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}