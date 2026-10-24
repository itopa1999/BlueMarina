using BlueMarina.Domain.Common;
using BlueMarina.Domain.Enums;

namespace BlueMarina.Domain.Entities.Onboarding;

/// <summary>
/// CRS tax residency declaration. Multiple rows per user.
/// </summary>
public class TaxResidency : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public string? Tin { get; set; }
    public TaxNoTinReason? NoTinReason { get; set; }
    public DateTimeOffset DeclaredAt { get; set; }
}
