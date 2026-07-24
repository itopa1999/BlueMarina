using System.ComponentModel.DataAnnotations.Schema;
using BlueMarina.Domain.Common;

namespace BlueMarina.Domain.Entities;

[Table("Otp Verification")]
public class OtpVerification : BaseEntity
{
    public Guid UserId { get; set; }

    public string OtpCode { get; set; } = string.Empty;

    public string Purpose { get; set; } = string.Empty;

    public bool IsUsed { get; set; }
    public DateTime? UsedAt { get; set; }

    public DateTime ExpiresAt { get; set; }
}