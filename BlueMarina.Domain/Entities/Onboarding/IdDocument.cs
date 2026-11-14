using System.ComponentModel.DataAnnotations.Schema;
using BlueMarina.Domain.Common;
using BlueMarina.Domain.Enums;

namespace BlueMarina.Domain.Entities.Onboarding;

/// <summary>
/// Scanned ID document.
/// </summary>

[Table("id_documents")]
public class IdDocument : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public IdDocumentType Type { get; set; }
    public string IdNumber { get; set; } = string.Empty;
    public string NameOnId { get; set; } = string.Empty;
    public DateOnly? ExpiryDate { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public IdDocumentStatus Status { get; set; }
    public int AttemptNumber { get; set; }
    public string? ReviewedByStaffUserId { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }
    public string? RejectionReason { get; set; }
}
