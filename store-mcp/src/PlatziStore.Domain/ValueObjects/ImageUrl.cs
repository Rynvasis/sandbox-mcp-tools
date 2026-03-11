using System;

namespace PlatziStore.Domain.ValueObjects;

public sealed record ImageUrl
{
    public string Value { get; init; }

    private ImageUrl(string value)
    {
        Value = value;
    }

    public static ImageUrl From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Image URL cannot be null or empty.", nameof(value));
        }

        var normalized = value.Trim();

        if (!Uri.TryCreate(normalized, UriKind.Absolute, out var uri))
        {
            throw new ArgumentException("Image URL must be a valid absolute URI.", nameof(value));
        }

        if (!string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Image URL must use http or https scheme.", nameof(value));
        }

        return new ImageUrl(normalized);
    }

    /// <summary>
    /// Safely creates an ImageUrl from a string, returning a fallback if the value is invalid.
    /// Use this when parsing data from untrusted external APIs.
    /// </summary>
    public static ImageUrl SafeFrom(string? value, string fallback = "https://example.com/placeholder.jpg")
    {
        if (string.IsNullOrWhiteSpace(value))
            return new ImageUrl(fallback);

        var normalized = value.Trim();

        if (!Uri.TryCreate(normalized, UriKind.Absolute, out var uri))
            return new ImageUrl(fallback);

        if (!string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            return new ImageUrl(fallback);

        return new ImageUrl(normalized);
    }

    public override string ToString() => Value;
}

