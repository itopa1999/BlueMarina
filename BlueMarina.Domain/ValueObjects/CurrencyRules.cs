
namespace BlueMarina.Domain.ValueObjects;

public static class CurrencyRules
{
    private static readonly IReadOnlyDictionary<string, int>
        DecimalPlacesByCurrency =
            new Dictionary<string, int>(
                StringComparer.OrdinalIgnoreCase)
            {
                ["NGN"] = 2,
                ["USD"] = 2,
                ["GBP"] = 2,
                ["EUR"] = 2,
                ["GHS"] = 2,
                ["KES"] = 2,
                ["ZAR"] = 2,
                ["JPY"] = 0,
                ["KRW"] = 0,
                ["BHD"] = 3,
                ["KWD"] = 3,
            };

    public static string NormalizeAndValidate(string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException(
                "Currency is required.",
                nameof(currency));

        var normalized = currency.Trim().ToUpperInvariant();

        if (!DecimalPlacesByCurrency.ContainsKey(normalized))
            throw new ArgumentException(
                $"Unsupported currency: {normalized}.",
                nameof(currency));

        return normalized;
    }

    public static int GetDecimalPlaces(string currency)
    {
        var normalized = NormalizeAndValidate(currency);
        return DecimalPlacesByCurrency[normalized];
    }

    public static long GetMinorUnitFactor(string currency)
    {
        var decimalPlaces = GetDecimalPlaces(currency);

        long factor = 1;

        for (var i = 0; i < decimalPlaces; i++)
            factor *= 10;

        return factor;
    }
}
