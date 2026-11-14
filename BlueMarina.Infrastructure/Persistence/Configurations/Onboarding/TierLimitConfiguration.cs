using BlueMarina.Domain.Entities.Onboarding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlueMarina.Infrastructure.Persistence.Configurations.Onboarding;

public class TierLimitConfiguration : IEntityTypeConfiguration<TierLimit>
{
    public void Configure(EntityTypeBuilder<TierLimit> builder)
    {
        builder.ToTable("tier_limits");

        builder.ComplexProperty(
            x => x.DailyLimit,
            money =>
            {
                money.Property(x => x.MinorUnits)
                    .HasColumnName("daily_limit_minor_units")
                    .IsRequired();

                money.Property(x => x.Currency)
                    .HasColumnName("daily_limit_currency")
                    .HasMaxLength(3)
                    .IsRequired();
            });

        builder.ComplexProperty(
            x => x.BalanceCap,
            money =>
            {
                money.Property(x => x.MinorUnits)
                    .HasColumnName("balance_cap_minor_units")
                    .IsRequired();

                money.Property(x => x.Currency)
                    .HasColumnName("balance_cap_currency")
                    .HasMaxLength(3)
                    .IsRequired();
            });
    }
}