using BlueMarina.Domain.Entities.Home;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlueMarina.Infrastructure.Persistence.Configurations.Home;

public sealed class BankConfiguration : IEntityTypeConfiguration<Bank>
{
    public void Configure(EntityTypeBuilder<Bank> builder)
    {
        builder.ToTable("banks");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(b => b.Code)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(b => b.Logo)
            .HasMaxLength(500);

        builder.HasIndex(b => b.Code)
            .IsUnique()
            .HasDatabaseName("ix_banks_code");

        builder.HasIndex(b => b.Name)
            .HasDatabaseName("ix_banks_name");
    }
}