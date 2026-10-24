using System.ComponentModel.DataAnnotations.Schema;
using BlueMarina.Domain.Common;
using BlueMarina.Domain.Enums;

namespace BlueMarina.Domain.Entities.Onboarding;

/// <summary>
/// Records who accepted which version, when and where.
/// </summary>

[Table("agreement_acceptances")]
public class AgreementAcceptance : BaseEntity
{
    public required string UserId { get; set; }
    public long AgreementVersionId { get; set; }
    public DateTimeOffset AcceptedAt { get; set; }
    public long? DeviceId { get; set; }
    public string? IpAddress { get; set; }
    public AgreementVersion AgreementVersion { get; set; } = null!;
}
