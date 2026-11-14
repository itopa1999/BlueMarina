
namespace BlueMarina.Domain.ValueObjects;

public readonly record struct Money
{
    public long MinorUnits { get; }

    public string Currency { get; }

    private Money(long minorUnits, string currency)
    {
        if (minorUnits < 0)
            throw new ArgumentOutOfRangeException(
                nameof(minorUnits),
                "Amount cannot be negative.");

        Currency = CurrencyRules.NormalizeAndValidate(currency);
        MinorUnits = minorUnits;
    }

    public static Money FromMajorUnits(
        decimal amount,
        string currency)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(
                nameof(amount),
                "Amount cannot be negative.");

        var normalizedCurrency =
            CurrencyRules.NormalizeAndValidate(currency);

        var minorUnitFactor =
            CurrencyRules.GetMinorUnitFactor(normalizedCurrency);

        var minorUnitsDecimal = amount * minorUnitFactor;

        if (minorUnitsDecimal != decimal.Truncate(minorUnitsDecimal))
            throw new ArgumentException(
                $"Amount cannot have more than " +
                $"{CurrencyRules.GetDecimalPlaces(normalizedCurrency)} " +
                "decimal places for this currency.",
                nameof(amount));

        return new Money(
            checked((long)minorUnitsDecimal),
            normalizedCurrency);
    }

    public static Money FromMinorUnits(
        long minorUnits,
        string currency)
    {
        return new Money(minorUnits, currency);
    }

    public decimal ToDecimal()
    {
        return MinorUnits /
            (decimal)CurrencyRules.GetMinorUnitFactor(Currency);
    }

    public static Money Zero(string currency)
    {
        return FromMinorUnits(0, currency);
    }

    public static Money operator +(Money left, Money right)
    {
        EnsureSameCurrency(left, right);

        return FromMinorUnits(
            checked(left.MinorUnits + right.MinorUnits),
            left.Currency);
    }

    public static Money operator -(Money left, Money right)
    {
        EnsureSameCurrency(left, right);

        if (left.MinorUnits < right.MinorUnits)
            throw new InvalidOperationException(
                "Money cannot become negative.");

        return FromMinorUnits(
            left.MinorUnits - right.MinorUnits,
            left.Currency);
    }

    private static void EnsureSameCurrency(
        Money left,
        Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException(
                $"Cannot operate on {left.Currency} and " +
                $"{right.Currency} without currency conversion.");
    }
}
