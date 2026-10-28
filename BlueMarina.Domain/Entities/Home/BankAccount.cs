using System.ComponentModel.DataAnnotations.Schema;
using BlueMarina.Domain.Common;
using BlueMarina.Shared.Constants;

namespace BlueMarina.Domain.Entities.Home;

/// <summary>
/// Payout or company bank account.
/// </summary>

[Table("bank_accounts")]
public class BankAccount : BaseEntity
{
    // public OwnerType OwnerType { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;

    public string BankCode { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string BankImageUrl { get; set; } = string.Empty;

    public string? PaystackRecipientCode { get; set; }
    public string? FlutterwaveRecipientId { get; set; }

    public DateTimeOffset? VerifiedAt { get; set; }
    public string? VerificationMessage { get; set; }

    public bool ConsentGiven { get; set; }
    public DateTimeOffset? ConsentGivenAt { get; set; }
    public string? ConsentVersion { get; set; }

    public string? Currency { get; set; } = CurrencyCode.NGN;
    public bool IsDefault { get; set; }
    public bool IsVerified { get; set; }
    public bool NameMatchesBvn { get; set; }
}
