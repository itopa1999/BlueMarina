using BlueMarina.Domain.Common;
using BlueMarina.Domain.Enums;

namespace BlueMarina.Domain.Entities.Onboarding;

/// <summary>
/// Records who accepted which version, when and where.
/// </summary>
public class AgreementAcceptance : BaseEntity
{
    public required string UserId { get; set; }
    public Guid AgreementVersionId { get; set; }
    public DateTimeOffset AcceptedAt { get; set; }
    public Guid? DeviceId { get; set; }
    public string? IpAddress { get; set; }
    public AgreementVersion AgreementVersion { get; set; } = null!;
}
