using System.ComponentModel.DataAnnotations.Schema;
using BlueMarina.Domain.Common;
using BlueMarina.Domain.Enums;

namespace BlueMarina.Domain.Entities.Security;

/// <summary>
/// Tracks lockout state across multiple OTP rows for the same (User, Purpose, Channel).
/// Survives OTP resends.
/// </summary>
[Table("otp_lockouts")]
public class OtpLockout : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public OtpPurpose Purpose { get; set; }
    public OtpChannel Channel { get; set; }
    public int FailedAttempts { get; set; }
    public DateTimeOffset? LockedUntil { get; set; }
    public DateTimeOffset? LastFailedAt { get; set; }
}