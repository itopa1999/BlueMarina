using System.ComponentModel.DataAnnotations.Schema;
using BlueMarina.Domain.Common;

namespace BlueMarina.Domain.Entities;

[Table("income_profiles")]
public class IncomeProfile : BaseEntity
{
    public long UserProfileId { get; set; }
    public decimal? AnnualIncome { get; set; }
    public RiskAppetite RiskAppetite { get; set; } = RiskAppetite.Moderate;

    [Column(TypeName = "jsonb")]
    public string? FinancialGoalsJson { get; set; }

    public virtual UserProfile UserProfile { get; set; } = null!;
}
