using System;

namespace PlatziStore.Domain.ValueObjects;

public sealed record MonetaryAmount
{
    public decimal Value { get; init; }

    private MonetaryAmount(decimal value)
    {
        Value = value;
    }

    public static MonetaryAmount From(decimal value)
    {
        if (value < 0)
        {
            throw new ArgumentException("Monetary amount cannot be negative.", nameof(value));
        }

        return new MonetaryAmount(value);
    }

    public override string ToString() => Value.ToString("0.##");
}

