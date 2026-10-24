using BlueMarina.Domain.Common;
using BlueMarina.Domain.Enums;

namespace BlueMarina.Domain.Entities;

/// <summary>
/// Every sign-in attempt, successful or not.
/// </summary>
public class SignInAttempt : BaseEntity
{
    public Guid? UserId { get; set; }
    public Guid? DeviceId { get; set; }
    public SignInMethod Method { get; set; }
    public SignInStatus Status { get; set; }
    public string? IpAddress { get; set; }
    public string? Location { get; set; }
}
