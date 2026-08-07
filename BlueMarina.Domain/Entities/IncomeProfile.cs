using System.ComponentModel.DataAnnotations.Schema;
using BlueMarina.Domain.Common;

namespace BlueMarina.Domain.Entities;

[Table("Income Profile")]
public class IncomeProfile : BaseEntity
{
    public Guid UserId { get; set; }
    public decimal? AnnualIncome { get; set; }
    public RiskAppetite RiskAppetite { get; set; } = RiskAppetite.Moderate;

    [Column(TypeName = "jsonb")]
    public string? FinancialGoalsJson { get; set; }

    public virtual UserProfile UserProfile { get; set; } = null!;
}
