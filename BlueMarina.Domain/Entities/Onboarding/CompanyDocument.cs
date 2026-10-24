using BlueMarina.Domain.Common;
using BlueMarina.Domain.Enums;

namespace BlueMarina.Domain.Entities;

/// <summary>
/// Company onboarding documents checked against CAC.
/// </summary>
public class CompanyDocument : BaseEntity
{
    public Guid CompanyId { get; set; }
    public CompanyDocumentType Type { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public CompanyDocumentStatus Status { get; set; }
    public string? RejectionReason { get; set; }
}
