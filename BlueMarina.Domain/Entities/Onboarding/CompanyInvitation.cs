using BlueMarina.Domain.Common;
using BlueMarina.Domain.Enums;

namespace BlueMarina.Domain.Entities;

/// <summary>
/// Secure invitation to join a company. Expires after 7 days.
/// </summary>
public class CompanyInvitation : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Guid CompanyMemberId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public InvitationStatus Status { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? OpenedAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public string? AcceptedWithEmail { get; set; }
}
