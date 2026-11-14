using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BlueMarina.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace BlueMarina.Infrastructure.Identity;

[Table("users")]
public class User : IdentityUser<long>
{
    [MaxLength(100)]
    public string PublicId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string OtherName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateOnly? DateOfBirth { get; set; }
    public Gender? Gender { get; set; }
    public string? BvnEncrypted { get; set; }
    public string? BvnHash { get; set; }
    public string? NinEncrypted { get; set; }
    public string? BvnPhone { get; set; }
    public string? Title { get; set; }
    public string? Nationality { get; set; }
    public string? StateOfOrigin { get; set; }
    public string? LgaOfOrigin { get; set; }
    public string? MothersMaidenName { get; set; }
    public string? Occupation { get; set; }
    public string? Employer { get; set; }
    public bool IsPep { get; set; }
    public string? PassportPhotoUrl { get; set; }
    public string OwnReferralCode { get; set; } = string.Empty;
    public string? ReferredByCode { get; set; }
    public long? ReferredByUserId { get; set; }
    public AccountTier Tier { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }


    
    public UserStatus Status { get; set; }
    public AccountRestrictionReason? RestrictionReason { get; set; } = AccountRestrictionReason.None;

    [MaxLength(1000)]
    public string? RestrictionReasonDetails { get; set; }

    public DateTimeOffset? RestrictedAt { get; set; }

    [MaxLength(100)]
    public string? RestrictedBy { get; set; }

    public DateTimeOffset? RestrictionExpiresAt { get; set; }

    public DateTimeOffset? ReactivatedAt { get; set; }



    // ─────────────────────────────────────────────────────────
    // Audit
    // ─────────────────────────────────────────────────────────

    public DateTimeOffset? CreatedAt { get; set; }

    public DateTimeOffset? ModifiedAt { get; set; }

    public bool IsDeleted { get; set; }

    public DateTimeOffset? DeletedAt { get; set; }

    [MaxLength(100)]
    public string? CreatedBy { get; set; }

    [MaxLength(100)]
    public string? ModifiedBy { get; set; }

    [MaxLength(100)]
    public string? DeletedBy { get; set; }
}
