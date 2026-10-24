using BlueMarina.Domain.Common;
using BlueMarina.Domain.Enums;

namespace BlueMarina.Domain.Entities;

/// <summary>
/// Liveness selfie check.
/// </summary>
public class SelfieCheck : BaseEntity
{
    public Guid UserId { get; set; }
    public string? MediaUrl { get; set; }
    public SelfieCheckStatus Status { get; set; }
    public int AttemptNumber { get; set; }
    public DateTime? ReviewDueBy { get; set; }
    public Guid? ReviewedByStaffId { get; set; }
}
