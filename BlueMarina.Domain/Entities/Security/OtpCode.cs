using System.ComponentModel.DataAnnotations.Schema;
using BlueMarina.Domain.Common;
using BlueMarina.Domain.Enums;

namespace BlueMarina.Domain.Entities.Security;

[Table("otp_codes")]
public class OtpCode : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string CodeHash { get; set; } = string.Empty;
    public OtpChannel Channel { get; set; }
    public OtpPurpose Purpose { get; set; }
    public bool IsUsed { get; set; }
    public DateTimeOffset? UsedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }    
    public int Attempts { get; set; }
    public DateTimeOffset? LockedUntil { get; set; }
    public DateTimeOffset? ConsumedAt { get; set; }
}