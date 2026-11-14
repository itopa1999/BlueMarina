using System.ComponentModel.DataAnnotations.Schema;
using BlueMarina.Domain.Common;

namespace BlueMarina.Domain.Entities.Onboarding;

/// <summary>
/// Specimen signature.
/// </summary>

[Table("signatures")]
public class Signature : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string SpecimenImageUrl { get; set; } = string.Empty;
    public string? SignedCopyUrl { get; set; }
    public DateTimeOffset SignedAt { get; set; }
}
