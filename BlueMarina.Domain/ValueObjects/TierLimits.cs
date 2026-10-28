using BlueMarina.Domain.Enums;
using BlueMarina.Shared.Constants;

namespace BlueMarina.Domain.ValueObjects;

public static class TierLimits
{
    // NGN defaults (in minor units = kobo)
    public const long Tier1DailyLimitNgnMinor = 5_000_000;    // ₦50,000
    public const long Tier1BalanceCapNgnMinor = 30_000_000;   // ₦300,000

    public const long Tier2DailyLimitNgnMinor = 100_000_000;  // ₦1,000,000
    public const long Tier2BalanceCapNgnMinor = 500_000_000;  // ₦5,000,000

    public const long Tier3DailyLimitNgnMinor = long.MaxValue;
    
    // ✅ Sentinel value for "no cap" (instead of null)
    public const long Tier3BalanceCapNgnMinor = long.MaxValue;

    // USD defaults (in minor units = cents)
    public const long Tier1DailyLimitUsdMinor = 10_000;       // $100
    public const long Tier1BalanceCapUsdMinor = 50_000;       // $500

    public const long Tier2DailyLimitUsdMinor = 200_000;      // $2,000
    public const long Tier2BalanceCapUsdMinor = 1_000_000;    // $10,000

    public const long Tier3DailyLimitUsdMinor = long.MaxValue;
    
    // ✅ Sentinel value for "no cap" (instead of null)
    public const long Tier3BalanceCapUsdMinor = long.MaxValue;

    public static long DailyLimitMinor(AccountTier tier, string currency) =>
        (tier, currency.ToUpperInvariant()) switch
        {
            (AccountTier.Tier1, CurrencyCode.NGN) => Tier1DailyLimitNgnMinor,
            (AccountTier.Tier2, CurrencyCode.NGN) => Tier2DailyLimitNgnMinor,
            (AccountTier.Tier3, CurrencyCode.NGN) => Tier3DailyLimitNgnMinor,

            (AccountTier.Tier1, CurrencyCode.USD) => Tier1DailyLimitUsdMinor,
            (AccountTier.Tier2, CurrencyCode.USD) => Tier2DailyLimitUsdMinor,
            (AccountTier.Tier3, CurrencyCode.USD) => Tier3DailyLimitUsdMinor,

            _ => throw new ArgumentException($"No daily limit configured for {tier} in {currency}.")
        };

    public static long BalanceCapMinor(AccountTier tier, string currency) =>
        (tier, currency.ToUpperInvariant()) switch
        {
            (AccountTier.Tier1, CurrencyCode.NGN) => Tier1BalanceCapNgnMinor,
            (AccountTier.Tier2, CurrencyCode.NGN) => Tier2BalanceCapNgnMinor,
            (AccountTier.Tier3, CurrencyCode.NGN) => Tier3BalanceCapNgnMinor,

            (AccountTier.Tier1, CurrencyCode.USD) => Tier1BalanceCapUsdMinor,
            (AccountTier.Tier2, CurrencyCode.USD) => Tier2BalanceCapUsdMinor,
            (AccountTier.Tier3, CurrencyCode.USD) => Tier3BalanceCapUsdMinor,

            _ => throw new ArgumentException($"No balance cap configured for {tier} in {currency}.")
        };

    // Feature flags (still code-driven — safer for business logic)
    public static bool CanInvest(AccountTier tier) => tier >= AccountTier.Tier2;
    public static bool CanWithdraw(AccountTier tier) => tier >= AccountTier.Tier2;
    public static bool CanBorrow(AccountTier tier) => tier >= AccountTier.Tier2;
    public static bool DollarProductsOpen(AccountTier tier) => tier >= AccountTier.Tier3;
    public static bool IpoOpen(AccountTier tier) => tier >= AccountTier.Tier3;
}