using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BlueMarina.Domain.Common;

namespace BlueMarina.Domain.Entities;


[Table("kyc_documents")]
public class KycDocument : BaseEntity
{
    public long UserProfileId { get; set; }

    public DocumentType Type { get; set; }

    [MaxLength(500)]
    public string FileUrl { get; set; } = string.Empty;

    // JSON data from OCR engine
    [Column(TypeName = "jsonb")]
    public string? OcrExtractedData { get; set; }

    public DocumentStatus Status { get; set; } = DocumentStatus.Pending;

    [MaxLength(250)]
    public string? RejectionReason { get; set; }

    public DateTime? VerifiedAt { get; set; }

    public long? VerifiedByUserId { get; set; }
    public bool IsExpired { get; set; }
    public virtual UserProfile UserProfile { get; set; } = null!;

}