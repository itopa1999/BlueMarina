using BlueMarina.Domain.Common;
using BlueMarina.Domain.Enums;

namespace BlueMarina.Domain.Entities;

/// <summary>
/// BVN or NIN verification attempt.
/// </summary>
public class IdentityCheck : BaseEntity
{
    public Guid UserId { get; set; }
    public IdentityCheckType Type { get; set; }
    public IdentityCheckStatus Status { get; set; }
    public string? ProviderReference { get; set; }
    public string? FailureReason { get; set; }
    public DateTime? CheckedAt { get; set; }
}
