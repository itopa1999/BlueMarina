using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BlueMarina.Domain.Common;

namespace BlueMarina.Domain.Entities;


[Table("Kyc Document")]
public class KycDocument : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual UserProfile UserProfile { get; set; } = null!;

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

    public Guid? VerifiedByUserId { get; set; } // admin or automated system

    public bool IsExpired { get; set; }
}