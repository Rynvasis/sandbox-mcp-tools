using System;
using System.Net.Mail;

namespace PlatziStore.Domain.ValueObjects;

public sealed record EmailAddress
{
    public string Value { get; init; }

    private EmailAddress(string value)
    {
        Value = value;
    }

    public static EmailAddress From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Email address cannot be null or empty.", nameof(value));
        }

        var normalized = value.Trim();

        try
        {
            var _ = new MailAddress(normalized);
        }
        catch (FormatException ex)
        {
            throw new ArgumentException("Email address is not in a valid format.", nameof(value), ex);
        }

        return new EmailAddress(normalized);
    }

    public override string ToString() => Value;
}

