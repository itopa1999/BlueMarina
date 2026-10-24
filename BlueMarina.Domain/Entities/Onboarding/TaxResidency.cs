using BlueMarina.Domain.Common;
using BlueMarina.Domain.Enums;

namespace BlueMarina.Domain.Entities;

/// <summary>
/// CRS tax residency declaration. Multiple rows per user.
/// </summary>
public class TaxResidency : BaseEntity
{
    public Guid UserId { get; set; }
    public string CountryCode { get; set; } = string.Empty;
    public string? Tin { get; set; }
    public TaxNoTinReason? NoTinReason { get; set; }
    public DateTime DeclaredAt { get; set; }
}
