using BlueMarina.Domain.Common;
using BlueMarina.Domain.Enums;

namespace BlueMarina.Domain.Entities;

/// <summary>
/// Signed-in session.
/// </summary>
public class Session : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid DeviceId { get; set; }
    public string RefreshTokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public SessionRevokeReason? RevokeReason { get; set; }
}
