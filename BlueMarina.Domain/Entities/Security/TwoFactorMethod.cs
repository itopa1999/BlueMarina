using BlueMarina.Domain.Common;
using BlueMarina.Domain.Enums;

namespace BlueMarina.Domain.Entities.Security;

/// <summary>
/// SMS or authenticator app second factor.
/// </summary>
public class TwoFactorMethod : BaseEntity
{
    public long UserId { get; set; }
    public TwoFactorType Type { get; set; }
    public string? SecretEncrypted { get; set; }
    public bool IsPrimary { get; set; }
    public DateTimeOffset EnabledAt { get; set; }
}
