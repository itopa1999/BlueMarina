using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BlueMarina.Domain.Common;

namespace BlueMarina.Domain.Entities;

[Table("User Profile")]
public class UserProfile : BaseEntity
{
    public Guid UserId { get; set; }
    
    // Personal details
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? OtherNames { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public Gender Gender { get; set; }

    public MaritalStatus MaritalStatus { get; set; }

    [MaxLength(50)]
    public string? Nationality { get; set; }

    // Employment & income
    [MaxLength(50)]
    public string? Occupation { get; set; }

    [MaxLength(100)]
    public string? Employer { get; set; }

    public decimal? AnnualIncome { get; set; }

    // BVN & NIN (encrypted in database – use built‑in encryption or Always Encrypted)
    [MaxLength(11)]
    public string? Bvn { get; set; }

    [MaxLength(11)]
    public string? Nin { get; set; }

    // Risk profile
    public RiskAppetite RiskAppetite { get; set; } = RiskAppetite.Moderate;

    // JSON field for financial goals (flexible)
    [Column(TypeName = "jsonb")] // for PostgreSQL; use nvarchar(max) for SQL Server
    public string? FinancialGoalsJson { get; set; }

    // KYC level (derived from verification status)
    public KycLevel KycLevel { get; set; } = KycLevel.Basic;

    public DateTime? KycVerifiedAt { get; set; }

    // Next‑of‑kin (basic info; can be expanded)
    [MaxLength(100)]
    public string? NextOfKinName { get; set; }

    [MaxLength(20)]
    public string? NextOfKinPhone { get; set; }

    [MaxLength(50)]
    public string? NextOfKinRelationship { get; set; }

    // Referral tracking
    public string? ReferralCode { get; set; } // unique per user

    public Guid? ReferredByUserId { get; set; }

    public string FullName => string.Join(" ", new[] { FirstName, LastName }
        .Where(value => !string.IsNullOrWhiteSpace(value)));

    // Navigation collections
    public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();
    public virtual ICollection<BankAccount> BankAccounts { get; set; } = new List<BankAccount>();
    public virtual ICollection<KycDocument> KycDocuments { get; set; } = new List<KycDocument>();
    public virtual ICollection<KycVerification> KycVerifications { get; set; } = new List<KycVerification>();
}