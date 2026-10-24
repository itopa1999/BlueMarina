using BlueMarina.Domain.Common;
using BlueMarina.Domain.Enums;

namespace BlueMarina.Domain.Entities;

/// <summary>
/// A phone the user has signed in on.
/// </summary>
public class Device : BaseEntity
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public string? Model { get; set; }
    public string? PushToken { get; set; }
    public bool IsTrusted { get; set; }
    public DateTime? TrustedSince { get; set; }
    public DateTime FirstSeenAt { get; set; }
    public DateTime LastSeenAt { get; set; }
    public string? LastLocation { get; set; }
    public DateTime? RemovedAt { get; set; }
}
