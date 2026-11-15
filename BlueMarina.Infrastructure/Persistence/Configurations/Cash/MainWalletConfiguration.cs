using BlueMarina.Domain.Entities.Cash;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlueMarina.Infrastructure.Persistence.Configurations.Cash;

public class MainWalletConfiguration : IEntityTypeConfiguration<MainWallet>
{
    public void Configure(EntityTypeBuilder<MainWallet> builder)
    {
        builder.ToTable("main_wallets");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.UserId)
            .IsRequired();

        builder.ComplexProperty(
            x => x.BalanceMinor,
            money =>
            {
                money.Property(x => x.MinorUnits)
                    .HasColumnName("balance_minor_units")
                    .IsRequired();

                money.Property(x => x.Currency)
                    .HasColumnName("balance_currency")
                    .HasMaxLength(3)
                    .IsRequired();
            });

        builder.HasIndex(w => new
        {
            w.UserId
        }).IsUnique();
    }
}