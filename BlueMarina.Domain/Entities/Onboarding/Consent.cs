using BlueMarina.Domain.Common;
using BlueMarina.Domain.Enums;

namespace BlueMarina.Domain.Entities.Onboarding;

/// <summary>
/// Optional consents and declarations (marketing, CRS).
/// </summary>
public class Consent : BaseEntity
{
    public long UserId { get; set; }
    // public ConsentType Type { get; set; }
    public string? Version { get; set; }
    public bool Granted { get; set; }
}
