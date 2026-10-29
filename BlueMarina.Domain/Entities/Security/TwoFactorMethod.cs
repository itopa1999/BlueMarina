using System.ComponentModel.DataAnnotations.Schema;
using BlueMarina.Domain.Common;
using BlueMarina.Domain.Enums;

namespace BlueMarina.Domain.Entities.Security;

/// <summary>
/// SMS or authenticator app second factor.
/// </summary>

[Table("two_factor_methods")]
public class TwoFactorMethod : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public TwoFactorType? Type { get; set; }
    public string? SecretEncrypted { get; set; }
    public bool IsPrimary { get; set; }
    public DateTimeOffset EnabledAt { get; set; }
    public ICollection<TwoFactorBackupCode> BackupCodes { get; set; } = new List<TwoFactorBackupCode>();
}


[Table("two_factor_backup_codes")]
public class TwoFactorBackupCode : BaseEntity
{
    public long TwoFactorMethodId { get; set; }
    public TwoFactorMethod TwoFactorMethod { get; set; } = null!;
    public string CodeHash { get; set; } = string.Empty;
    public DateTimeOffset? UsedAt { get; set; }
}