using System.ComponentModel.DataAnnotations.Schema;
using BlueMarina.Domain.Common;
using BlueMarina.Domain.Enums;
using BlueMarina.Domain.ValueObjects;

namespace BlueMarina.Domain.Entities.Onboarding;

[Table("tier_limits")]
public class TierLimit : BaseEntity
{
    public AccountTier Tier { get; set; }
    public Money DailyLimit { get; set; }
    public Money BalanceCap { get; set; }
    public bool CanInvest { get; set; }
    public bool CanWithdraw { get; set; }
    public bool CanBorrow { get; set; }
    public bool DollarWalletEnabled { get; set; }
    public bool IpoEnabled { get; set; }
}