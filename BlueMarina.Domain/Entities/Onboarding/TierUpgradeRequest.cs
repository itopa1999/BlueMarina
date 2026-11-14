using System.ComponentModel.DataAnnotations.Schema;
using BlueMarina.Domain.Common;
using BlueMarina.Domain.Enums;

namespace BlueMarina.Domain.Entities.Onboarding;

/// <summary>
/// Request to move to a higher tier.
/// </summary>

[Table("tier_upgrade_requests")]
public class TierUpgradeRequest : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public AccountTier TargetTier { get; set; }
    public TierUpgradeStatus Status { get; set; }
    public DateTimeOffset? SubmittedAt { get; set; }
    public DateTimeOffset? ReviewDueBy { get; set; }
    public DateTimeOffset? DecidedAt { get; set; }
    public List<TierUpgradeItem> Items { get; set; } = new();
}
