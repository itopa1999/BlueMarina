using System.ComponentModel.DataAnnotations.Schema;
using BlueMarina.Domain.Common;

namespace BlueMarina.Domain.Entities.Onboarding;

[Table("lgas")]
public class Lga : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public long StateId { get; set; }
    public State State { get; set; } = null!;
}