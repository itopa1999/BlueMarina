using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BlueMarina.Domain.Common;

namespace BlueMarina.Domain.Entities;

[Table("next_of_kins")]
public class NextOfKin : BaseEntity
{
    public long UserProfileId { get; set; }

    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(20)]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Relationship { get; set; } = string.Empty;

    public virtual UserProfile UserProfile { get; set; } = null!;
}
