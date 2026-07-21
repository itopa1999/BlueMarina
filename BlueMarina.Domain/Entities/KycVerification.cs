using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BlueMarina.Domain.Common;

namespace BlueMarina.Domain.Entities;

[Table("Bank Account")]
public class KycVerification : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual UserProfile UserProfile { get; set; } = null!;

    public VerificationType Type { get; set; }

    public VerificationResult Result { get; set; }

    [MaxLength(500)]
    public string? Details { get; set; } // JSON with extra info

    public DateTime? CompletedAt { get; set; }

    public Guid? InitiatedByUserId { get; set; } // could be system or admin
}