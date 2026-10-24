using BlueMarina.Domain.Common;
using BlueMarina.Domain.Enums;

namespace BlueMarina.Domain.Entities.Onboarding;

/// <summary>
/// Director, signatory, owner or admin of a company.
/// </summary>
public class CompanyMember : BaseEntity
{
    public long CompanyId { get; set; }
    public long? UserId { get; set; }
    public CompanyMemberType MemberType { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public CompanyRole Roles { get; set; }
    public SigningMandate? SigningMandate { get; set; }
    public decimal? OwnershipPercent { get; set; }
    public MemberVerificationStatus VerificationStatus { get; set; }
    public Company Company { get; set; } = null!;
}
