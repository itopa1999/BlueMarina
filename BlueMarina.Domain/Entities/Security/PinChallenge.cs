using System.ComponentModel.DataAnnotations.Schema;
using BlueMarina.Domain.Common;
using BlueMarina.Domain.Enums;
namespace BlueMarina.Domain.Entities.Security;

[Table("pin_challenges")]
public class PinChallenge : BaseEntity
{
    public required string UserId { get; set; }
    public PinChallengeType PinChallengeType { get; set; }
    public bool IsUsed { get; set; }
    public DateTimeOffset UsedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; } = DateTimeOffset.UtcNow.AddMinutes(2);
    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;
}