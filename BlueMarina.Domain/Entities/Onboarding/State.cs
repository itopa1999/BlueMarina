using System.ComponentModel.DataAnnotations.Schema;
using BlueMarina.Domain.Common;

namespace BlueMarina.Domain.Entities.Onboarding;

[Table("states")]
public class State : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    
    public ICollection<Lga> Lgas { get; set; } = new List<Lga>();
}