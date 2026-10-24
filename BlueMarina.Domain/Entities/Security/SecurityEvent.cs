using BlueMarina.Domain.Common;
using BlueMarina.Domain.Enums;

namespace BlueMarina.Domain.Entities;

/// <summary>
/// Audit trail for security-relevant events.
/// </summary>
public class SecurityEvent : BaseEntity
{
    public Guid UserId { get; set; }
    public SecurityEventType Type { get; set; }
    public Guid? DeviceId { get; set; }
    public string? IpAddress { get; set; }
    public string? Location { get; set; }
    public string? MetadataJson { get; set; }
}
