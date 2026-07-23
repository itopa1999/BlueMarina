using System.ComponentModel.DataAnnotations;

namespace BlueMarina.Domain.Common;

public abstract class BaseEntity
{
    [Key]
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    [MaxLength(100)]
    public string? CreatedBy { get; set; }

    [MaxLength(100)]
    public string? ModifiedBy { get; set; }

    [MaxLength(100)]
    public string? DeletedBy { get; set; }
}