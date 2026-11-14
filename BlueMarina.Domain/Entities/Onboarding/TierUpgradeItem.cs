using System.ComponentModel.DataAnnotations.Schema;
using BlueMarina.Domain.Common;
using BlueMarina.Domain.Enums;

namespace BlueMarina.Domain.Entities.Onboarding;

/// <summary>
/// Per-step review result inside a tier upgrade.
/// </summary>

[Table("tier_upgrade_items")]
public class TierUpgradeItem : BaseEntity
{
    public long TierUpgradeRequestId { get; set; }
    public TierUpgradeItemType ItemType { get; set; }
    public ReviewItemStatus Status { get; set; }
    public string? Reason { get; set; }
}
