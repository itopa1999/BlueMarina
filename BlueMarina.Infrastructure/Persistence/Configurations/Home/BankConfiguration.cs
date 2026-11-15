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

        builder.Property(b => b.Slug)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(b => b.LongCode)
            .HasMaxLength(30);

        builder.Property(b => b.Currency)
            .IsRequired()
            .HasMaxLength(3)
            .HasDefaultValue("NGN");

        builder.Property(b => b.Country)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Nigeria");

        builder.Property(b => b.Logo)
            .HasMaxLength(500);

        builder.Property(b => b.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(b => b.SupportsTransfer)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(b => b.SupportsDirectDebit)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(b => new
        {
            b.Code,
            b.Currency
        })
        .IsUnique()
        .HasDatabaseName("ix_banks_code_currency");

        builder.HasIndex(b => b.Name)
            .HasDatabaseName("ix_banks_name");

        builder.HasIndex(b => b.Slug)
            .HasDatabaseName("ix_banks_slug");
    }
}