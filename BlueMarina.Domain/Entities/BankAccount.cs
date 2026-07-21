using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BlueMarina.Domain.Common;

namespace BlueMarina.Domain.Entities;

[Table("Bank Account")]
public class BankAccount : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual UserProfile UserProfile { get; set; } = null!;

    [MaxLength(10)]
    public string BankCode { get; set; } = string.Empty;

    [MaxLength(50)]
    public string BankName { get; set; } = string.Empty;

    [MaxLength(20)]
    public string AccountNumber { get; set; } = string.Empty;

    [MaxLength(100)]
    public string AccountName { get; set; } = string.Empty;

    public Currency Currency { get; set; } // NGN, USD, etc.

    public bool IsDefault { get; set; }

    public bool IsVerified { get; set; } // via NIBSS name‑enquiry

    public bool IsActive { get; set; } = true;
}