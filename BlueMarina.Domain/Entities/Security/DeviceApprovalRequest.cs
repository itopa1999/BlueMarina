using BlueMarina.Domain.Common;
using BlueMarina.Domain.Enums;

namespace BlueMarina.Domain.Entities.Security;

/// <summary>
/// New-phone approval from a trusted phone. Expires after 5 minutes.
/// </summary>
public class DeviceApprovalRequest : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid NewDeviceId { get; set; }
    public Guid? ApproverDeviceId { get; set; }
    public DeviceApprovalStatus Status { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? DecidedAt { get; set; }
}
