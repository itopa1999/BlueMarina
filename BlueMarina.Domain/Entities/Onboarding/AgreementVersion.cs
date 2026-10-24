using BlueMarina.Domain.Common;
using BlueMarina.Domain.Enums;

namespace BlueMarina.Domain.Entities;

/// <summary>
/// A published version of terms, privacy policy or account agreement.
/// </summary>
public class AgreementVersion : BaseEntity
{
    public AgreementType Type { get; set; }
    public string Version { get; set; } = string.Empty;
    public DateTime EffectiveAt { get; set; }
    public string ContentUrl { get; set; } = string.Empty;
    public bool IsCurrent { get; set; }
}
