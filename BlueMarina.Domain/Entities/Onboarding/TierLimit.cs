using BlueMarina.Domain.Common;
using BlueMarina.Domain.Enums;

namespace BlueMarina.Domain.Entities;

/// <summary>
/// Limits and feature flags per tier. Configuration table.
/// </summary>
public class TierLimit : BaseEntity
{
    public AccountTier Tier { get; set; }
    public long DailyLimitMinor { get; set; }
    public long? BalanceCapMinor { get; set; }
    public bool CanInvest { get; set; }
    public bool CanWithdraw { get; set; }
    public bool CanBorrow { get; set; }
    public bool DollarWalletEnabled { get; set; }
    public bool IpoEnabled { get; set; }
}
