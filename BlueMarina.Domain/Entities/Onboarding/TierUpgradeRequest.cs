using BlueMarina.Domain.Common;
using BlueMarina.Domain.Enums;

namespace BlueMarina.Domain.Entities;

/// <summary>
/// Request to move to a higher tier.
/// </summary>
public class TierUpgradeRequest : BaseEntity
{
    public Guid UserId { get; set; }
    public AccountTier TargetTier { get; set; }
    public TierUpgradeStatus Status { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ReviewDueBy { get; set; }
    public DateTime? DecidedAt { get; set; }
    public List<TierUpgradeItem> Items { get; set; } = new();
}
