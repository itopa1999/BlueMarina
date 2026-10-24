using System.ComponentModel.DataAnnotations.Schema;
using BlueMarina.Domain.Common;

namespace BlueMarina.Domain.Entities;

[Table("otp_verifications")]
public class OtpVerification : BaseEntity
{
    public long UserProfileId { get; set; }
    public string OtpCode { get; set; } = string.Empty;

    public string Purpose { get; set; } = string.Empty;

    public bool IsUsed { get; set; }
    public DateTime? UsedAt { get; set; }

    public DateTime ExpiresAt { get; set; }
    public virtual UserProfile UserProfile { get; set; } = null!;
}