using System.ComponentModel.DataAnnotations.Schema;
using BlueMarina.Domain.Common;
using BlueMarina.Domain.Enums;

namespace BlueMarina.Domain.Entities.Onboarding;

/// <summary>
/// Liveness selfie check.
/// </summary>

[Table("selfie_checks")]
public class SelfieCheck : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string? MediaUrl { get; set; }
    public SelfieCheckStatus Status { get; set; }
    public int AttemptNumber { get; set; }
    public DateTimeOffset? ReviewDueBy { get; set; }
    public string? ReviewedByStaffUserId { get; set; }

    public string? RejectionReason { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }
}
