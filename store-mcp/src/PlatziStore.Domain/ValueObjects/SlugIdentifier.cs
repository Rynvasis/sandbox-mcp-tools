using System;
using System.Text.RegularExpressions;

namespace PlatziStore.Domain.ValueObjects;

public sealed record SlugIdentifier
{
    private static readonly Regex SlugPattern =
        new("^(?=.*[a-z0-9])[a-z0-9-]+$", RegexOptions.Compiled);

    public string Value { get; init; }

    private SlugIdentifier(string value)
    {
        Value = value;
    }

    public static SlugIdentifier From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Slug cannot be null or empty.", nameof(value));
        }

        var normalized = value.Trim();

        if (!SlugPattern.IsMatch(normalized))
        {
            throw new ArgumentException(
                "Slug must match pattern [a-z0-9-]+ and contain at least one alphanumeric character.",
                nameof(value));
        }

        return new SlugIdentifier(normalized);
    }

    public override string ToString() => Value;
}

