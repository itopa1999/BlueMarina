using BlueMarina.Domain.Entities.Onboarding;
using BlueMarina.Domain.Enums;
using BlueMarina.Domain.ValueObjects;
using BlueMarina.Shared.Constants;

namespace BlueMarina.Infrastructure.Persistence.Seeding;

public static class DefaultTierLimit
{
    public static List<TierLimit> Create()
    {
        return
        [
            // NGN
            new TierLimit
            {
                Tier = AccountTier.Tier1,
                DailyLimit = Money.FromMinorUnits(TierLimits.Tier1DailyLimitNgnMinor, CurrencyCode.NGN),
                BalanceCap = Money.FromMinorUnits(TierLimits.Tier1BalanceCapNgnMinor, CurrencyCode.NGN),
                CanInvest = TierLimits.CanInvest(AccountTier.Tier1),
                CanWithdraw = TierLimits.CanWithdraw(AccountTier.Tier1),
                CanBorrow = TierLimits.CanBorrow(AccountTier.Tier1),
                DollarWalletEnabled = TierLimits.DollarProductsOpen(AccountTier.Tier1),
                IpoEnabled = TierLimits.IpoOpen(AccountTier.Tier1)
            },
            new TierLimit
            {
                Tier = AccountTier.Tier2,
                DailyLimit = Money.FromMinorUnits(TierLimits.Tier2DailyLimitNgnMinor, CurrencyCode.NGN),
                BalanceCap = Money.FromMinorUnits(TierLimits.Tier2BalanceCapNgnMinor, CurrencyCode.NGN),
                CanInvest = TierLimits.CanInvest(AccountTier.Tier2),
                CanWithdraw = TierLimits.CanWithdraw(AccountTier.Tier2),
                CanBorrow = TierLimits.CanBorrow(AccountTier.Tier2),
                DollarWalletEnabled = TierLimits.DollarProductsOpen(AccountTier.Tier2),
                IpoEnabled = TierLimits.IpoOpen(AccountTier.Tier2)
            },
            new TierLimit
            {
                Tier = AccountTier.Tier3,
                DailyLimit = Money.FromMinorUnits(TierLimits.Tier3DailyLimitNgnMinor, CurrencyCode.NGN),
                BalanceCap = Money.FromMinorUnits(long.MaxValue, CurrencyCode.NGN),
                CanInvest = TierLimits.CanInvest(AccountTier.Tier3),
                CanWithdraw = TierLimits.CanWithdraw(AccountTier.Tier3),
                CanBorrow = TierLimits.CanBorrow(AccountTier.Tier3),
                DollarWalletEnabled = TierLimits.DollarProductsOpen(AccountTier.Tier3),
                IpoEnabled = TierLimits.IpoOpen(AccountTier.Tier3)
            },

            // USD
            new TierLimit
            {
                Tier = AccountTier.Tier1,
                DailyLimit = Money.FromMinorUnits(TierLimits.Tier1DailyLimitUsdMinor, CurrencyCode.USD),
                BalanceCap = Money.FromMinorUnits(TierLimits.Tier1BalanceCapUsdMinor, CurrencyCode.USD),
                CanInvest = TierLimits.CanInvest(AccountTier.Tier1),
                CanWithdraw = TierLimits.CanWithdraw(AccountTier.Tier1),
                CanBorrow = TierLimits.CanBorrow(AccountTier.Tier1),
                DollarWalletEnabled = TierLimits.DollarProductsOpen(AccountTier.Tier1),
                IpoEnabled = TierLimits.IpoOpen(AccountTier.Tier1)
            },
            new TierLimit
            {
                Tier = AccountTier.Tier2,
                DailyLimit = Money.FromMinorUnits(TierLimits.Tier2DailyLimitUsdMinor, CurrencyCode.USD),
                BalanceCap = Money.FromMinorUnits(TierLimits.Tier2BalanceCapUsdMinor, CurrencyCode.USD),
                CanInvest = TierLimits.CanInvest(AccountTier.Tier2),
                CanWithdraw = TierLimits.CanWithdraw(AccountTier.Tier2),
                CanBorrow = TierLimits.CanBorrow(AccountTier.Tier2),
                DollarWalletEnabled = TierLimits.DollarProductsOpen(AccountTier.Tier2),
                IpoEnabled = TierLimits.IpoOpen(AccountTier.Tier2)
            },
            new TierLimit
            {
                Tier = AccountTier.Tier3,
                DailyLimit = Money.FromMinorUnits(TierLimits.Tier3DailyLimitUsdMinor, CurrencyCode.USD),
                BalanceCap = Money.FromMinorUnits(long.MaxValue, CurrencyCode.USD),
                CanInvest = TierLimits.CanInvest(AccountTier.Tier3),
                CanWithdraw = TierLimits.CanWithdraw(AccountTier.Tier3),
                CanBorrow = TierLimits.CanBorrow(AccountTier.Tier3),
                DollarWalletEnabled = TierLimits.DollarProductsOpen(AccountTier.Tier3),
                IpoEnabled = TierLimits.IpoOpen(AccountTier.Tier3)
            }
        ];
    }
}