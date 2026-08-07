using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BlueMarina.Domain.Common;

namespace BlueMarina.Domain.Entities;

[Table("employment_informations")]
public class EmploymentInformation : BaseEntity
{
    public long UserProfileId { get; set; }
    public virtual UserProfile UserProfile { get; set; } = null!;

    [MaxLength(50)]
    public string? Occupation { get; set; }

    [MaxLength(100)]
    public string? Employer { get; set; }
}
