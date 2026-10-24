using System.ComponentModel.DataAnnotations.Schema;
using BlueMarina.Domain.Common;

namespace BlueMarina.Domain.Entities.Security;

[Table("refresh_tokens")]
public class RefreshToken : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string TokenHash { get; set; } = string.Empty;

    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset? RevokedAt { get; set; }

    public string? RevokedByIp { get; set; }

    public string? ReplacedByTokenHash { get; set; }

    public string? RevocationReason { get; set; }

    public string? CreatedByIp { get; set; }

    public string? DeviceName { get; set; }

    public string? UserAgent { get; set; }

    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;

    public bool IsRevoked => RevokedAt.HasValue;

    public bool IsActive => !IsExpired && !IsRevoked;
}