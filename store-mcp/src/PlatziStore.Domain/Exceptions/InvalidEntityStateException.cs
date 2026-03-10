using System;

namespace PlatziStore.Domain.Exceptions;

public sealed class InvalidEntityStateException : Exception
{
    public string RuleDescription { get; }

    public InvalidEntityStateException(string ruleDescription)
        : base($"Entity is in an invalid state: {ruleDescription}")
    {
        RuleDescription = ruleDescription ?? throw new ArgumentNullException(nameof(ruleDescription));
    }
}

