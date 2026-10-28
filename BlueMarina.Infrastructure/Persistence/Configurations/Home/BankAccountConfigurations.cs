using BlueMarina.Domain.Entities.Home;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace BlueMarina.Infrastructure.Persistence.Configurations.Home;

public class BankAccountConfiguration
    : IEntityTypeConfiguration<BankAccount>
{
    public void Configure(EntityTypeBuilder<BankAccount> builder)
    {
        builder.ToTable("bank_accounts");

        builder.HasKey(x => x.Id);

        // User relationship
        builder.Property(x => x.UserId)
            .IsRequired();

        // Bank account details
        builder.Property(x => x.AccountNumber)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(x => x.AccountName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.BankCode)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(x => x.BankName)
            .IsRequired()
            .HasMaxLength(100);

        // Paystack recipient code
        builder.Property(x => x.PaystackRecipientCode)
            .HasMaxLength(100);

        builder.Property(x => x.IsDefault)
            .IsRequired()
            .HasDefaultValue(false);

        // Verification
        builder.Property(x => x.VerifiedAt);

        builder.Property(x => x.VerificationMessage)
            .HasMaxLength(500);

        builder.Property(x => x.ConsentGiven)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.ConsentGivenAt);

        builder.Property(x => x.ConsentVersion)
            .HasMaxLength(50);

        builder.Property(x => x.Currency)
            .IsRequired()
            .HasMaxLength(3)
            .HasDefaultValue("NGN");

        // Indexes
        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("IX_bank_accounts_UserId");

        builder.HasIndex(x => x.AccountNumber)
            .HasDatabaseName("IX_bank_accounts_AccountNumber");

        builder.HasIndex(x => x.PaystackRecipientCode)
            .HasDatabaseName("IX_bank_accounts_RecipientCode");

        builder.HasIndex(x => new { x.UserId, x.IsDefault })
            .HasDatabaseName("IX_bank_accounts_UserId_IsDefault")
            .IsUnique()
            .HasFilter("\"IsDefault\" = true");

        builder.HasIndex(x => new { x.UserId, x.AccountNumber })
            .HasDatabaseName("IX_bank_accounts_UserId_AccountNumber")
            .IsUnique();
    }
}