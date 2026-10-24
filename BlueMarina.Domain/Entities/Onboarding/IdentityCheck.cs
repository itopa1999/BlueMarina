using System.ComponentModel.DataAnnotations.Schema;
using BlueMarina.Domain.Common;
using BlueMarina.Domain.Enums;

namespace BlueMarina.Domain.Entities.Onboarding;

/// <summary>
/// BVN or NIN verification attempt.
/// </summary>

[Table("identity_checks")]
public class IdentityCheck : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public IdentityCheckType Type { get; set; }
    public IdentityCheckStatus Status { get; set; }
    public string? ProviderReference { get; set; }
    public string? FailureReason { get; set; }
    public DateTimeOffset? CheckedAt { get; set; }
}
