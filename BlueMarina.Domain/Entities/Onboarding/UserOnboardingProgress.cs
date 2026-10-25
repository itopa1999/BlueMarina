using System.ComponentModel.DataAnnotations.Schema;
using BlueMarina.Domain.Common;

namespace BlueMarina.Domain.Entities.Onboarding;

/// <summary>
/// Tracks the onboarding progress and tier requirements for a user.
/// 1-to-1 relationship with the User entity.
/// </summary>
[Table("user_onboarding_progresses")]
public class UserOnboardingProgress : BaseEntity
{
    public string UserId { get; set; } = string.Empty;

    // Tier 1 Status
    public bool Tier1_AccountDone { get; set; }
    public bool Tier1_SelfieDone { get; set; }
    public bool Tier1_IdDone { get; set; }
    public bool Tier1_SignatureDone { get; set; }
    public bool Tier1_IsCompleted { get; set; }

    // Tier 2 Status
    public bool Tier2_AccountDone { get; set; }
    public bool Tier2_SelfieDone { get; set; }
    public bool Tier2_IdDone { get; set; }
    public bool Tier2_SignatureDone { get; set; }
    public bool Tier2_IsCompleted { get; set; }

    // Tier 3 Status
    public bool Tier3_AccountDone { get; set; }
    public bool Tier3_SelfieDone { get; set; }
    public bool Tier3_IdDone { get; set; }
    public bool Tier3_SignatureDone { get; set; }
    public bool Tier3_IsCompleted { get; set; }
}