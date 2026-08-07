using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BlueMarina.Domain.Common;

namespace BlueMarina.Domain.Entities;

[Table("kyc_profiles")]
public class KycProfile : BaseEntity
{
    public long UserProfileId { get; set; }
    [MaxLength(11)]
    public string? Bvn { get; set; }

    [MaxLength(11)]
    public string? Nin { get; set; }

    public KycLevel KycLevel { get; set; } = KycLevel.Basic;
    public DateTime? VerifiedAt { get; set; }

    public virtual UserProfile UserProfile { get; set; } = null!;
}
