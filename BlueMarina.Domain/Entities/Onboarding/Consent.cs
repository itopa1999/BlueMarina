using System.ComponentModel.DataAnnotations.Schema;
using BlueMarina.Domain.Common;
using BlueMarina.Domain.Enums;

namespace BlueMarina.Domain.Entities.Onboarding;

/// <summary>
/// Optional consents and declarations (marketing, CRS).
/// </summary>

[Table("consents")]
public class Consent : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public ConsentType Type { get; set; }
    public string? Version { get; set; }
    public bool Granted { get; set; }
}
