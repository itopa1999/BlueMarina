using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BlueMarina.Domain.Common;

namespace BlueMarina.Domain.Entities.Home;

[Table("banks")]
public class Bank : BaseEntity
{
    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Slug { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? LongCode { get; set; }

    [Required]
    [MaxLength(3)]
    public string Currency { get; set; } = "NGN";

    [Required]
    [MaxLength(50)]
    public string Country { get; set; } = "Nigeria";

    public bool IsActive { get; set; }

    [MaxLength(500)]
    public string? Logo { get; set; }
    
    public bool SupportsTransfer { get; set; }

    public bool SupportsDirectDebit { get; set; }
}