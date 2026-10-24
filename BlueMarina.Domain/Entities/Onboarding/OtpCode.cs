using BlueMarina.Domain.Common;
using BlueMarina.Domain.Enums;

namespace BlueMarina.Domain.Entities;

/// <summary>
/// One-time code for verification, sign-in, resets and contact changes.
/// </summary>
public class OtpCode : BaseEntity
{
    public Guid? UserId { get; set; }
    public OtpChannel Channel { get; set; }
    public OtpPurpose Purpose { get; set; }
    public string Destination { get; set; } = string.Empty;
    public string CodeHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public int Attempts { get; set; }
    public DateTime? LockedUntil { get; set; }
    public DateTime? ConsumedAt { get; set; }
}
