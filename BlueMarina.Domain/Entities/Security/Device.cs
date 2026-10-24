using BlueMarina.Domain.Common;
using BlueMarina.Domain.Enums;

namespace BlueMarina.Domain.Entities.Security;

/// <summary>
/// A phone the user has signed in on.
/// </summary>
public class Device : BaseEntity
{
    public long UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public string? Model { get; set; }
    public string? PushToken { get; set; }
    public bool IsTrusted { get; set; }
    public DateTimeOffset? TrustedSince { get; set; }
    public DateTimeOffset FirstSeenAt { get; set; }
    public DateTimeOffset LastSeenAt { get; set; }
    public string? LastLocation { get; set; }
    public DateTimeOffset? RemovedAt { get; set; }
}
