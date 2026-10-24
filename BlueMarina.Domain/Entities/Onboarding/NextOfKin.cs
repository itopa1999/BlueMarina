using BlueMarina.Domain.Common;
using BlueMarina.Domain.Enums;

namespace BlueMarina.Domain.Entities.Onboarding;

/// <summary>
/// Next of kin. No account access.
/// </summary>
public class NextOfKin : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public NextOfKinRelationship Relationship { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public bool LivesAtSameAddress { get; set; }
}
