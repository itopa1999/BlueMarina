using BlueMarina.Domain.Common;
using BlueMarina.Domain.Enums;

namespace BlueMarina.Domain.Entities;

/// <summary>
/// Per-step review result inside a tier upgrade.
/// </summary>
public class TierUpgradeItem : BaseEntity
{
    public Guid TierUpgradeRequestId { get; set; }
    public TierUpgradeItemType ItemType { get; set; }
    public ReviewItemStatus Status { get; set; }
    public string? Reason { get; set; }
}
