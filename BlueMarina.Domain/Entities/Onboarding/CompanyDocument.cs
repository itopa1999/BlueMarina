using System.ComponentModel.DataAnnotations.Schema;
using BlueMarina.Domain.Common;
using BlueMarina.Domain.Enums;

namespace BlueMarina.Domain.Entities.Onboarding;

/// <summary>
/// Company onboarding documents checked against CAC.
/// </summary>

[Table("company_documents")]
public class CompanyDocument : BaseEntity
{
    public long CompanyId { get; set; }
    public CompanyDocumentType Type { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public CompanyDocumentStatus Status { get; set; }
    public string? RejectionReason { get; set; }
}
