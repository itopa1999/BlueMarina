using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BlueMarina.Domain.Common;

namespace BlueMarina.Domain.Entities;

[Table("kyc_verifications")]
public class KycVerification : BaseEntity
{
    public long UserProfileId { get; set; }

    public VerificationType Type { get; set; }

    public VerificationResult Result { get; set; }

    [MaxLength(500)]
    public string? Details { get; set; } // JSON with extra info

    public DateTime? CompletedAt { get; set; }

    public long? InitiatedByUserId { get; set; } // could be system or admin

    public virtual UserProfile UserProfile { get; set; } = null!;

}