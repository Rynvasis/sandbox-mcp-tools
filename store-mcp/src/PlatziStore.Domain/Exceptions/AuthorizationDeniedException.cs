using System;

namespace PlatziStore.Domain.Exceptions;

public sealed class AuthorizationDeniedException : Exception
{
    public string Reason { get; }

    public AuthorizationDeniedException(string reason)
        : base($"Authorization denied: {reason}")
    {
        Reason = reason ?? throw new ArgumentNullException(nameof(reason));
    }
}

