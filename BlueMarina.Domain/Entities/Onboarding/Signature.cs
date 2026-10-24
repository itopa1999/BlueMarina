using BlueMarina.Domain.Common;
using BlueMarina.Domain.Enums;

namespace BlueMarina.Domain.Entities.Onboarding;

/// <summary>
/// Specimen signature.
/// </summary>
public class Signature : BaseEntity
{
    public long UserId { get; set; }
    public string SpecimenImageUrl { get; set; } = string.Empty;
    public string? SignedCopyUrl { get; set; }
    public DateTimeOffset SignedAt { get; set; }
}
