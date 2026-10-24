using System.ComponentModel.DataAnnotations.Schema;
using BlueMarina.Domain.Common;
using BlueMarina.Domain.Enums;

namespace BlueMarina.Domain.Entities.Onboarding;

/// <summary>
/// Secure invitation to join a company. Expires after 7 days.
/// </summary>

[Table("company_invitations")]
public class CompanyInvitation : BaseEntity
{
    public long CompanyId { get; set; }
    public string CompanyMemberUserId { get; set; } = string.Empty;
    public string TokenHash { get; set; } = string.Empty;
    public InvitationStatus Status { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? OpenedAt { get; set; }
    public DateTimeOffset? AcceptedAt { get; set; }
    public string? AcceptedWithEmail { get; set; }
}
