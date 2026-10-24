using BlueMarina.Domain.Common;
using BlueMarina.Domain.Enums;

namespace BlueMarina.Domain.Entities.Security;

/// <summary>
/// New-phone approval from a trusted phone. Expires after 5 minutes.
/// </summary>
public class DeviceApprovalRequest : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public long NewDeviceId { get; set; }
    public long? ApproverDeviceId { get; set; }
    public DeviceApprovalStatus Status { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? DecidedAt { get; set; }
}
