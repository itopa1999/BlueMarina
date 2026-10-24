using BlueMarina.Domain.Common;
using BlueMarina.Domain.Enums;

namespace BlueMarina.Domain.Entities.Onboarding;

/// <summary>
/// Liveness selfie check.
/// </summary>
public class SelfieCheck : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string? MediaUrl { get; set; }
    public SelfieCheckStatus Status { get; set; }
    public int AttemptNumber { get; set; }
    public DateTime? ReviewDueBy { get; set; }
    public long? ReviewedByStaffId { get; set; }
}
