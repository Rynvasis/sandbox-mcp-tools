using PlatziStore.Domain.ValueObjects;

namespace PlatziStore.Domain.Entities;

public sealed class StoreCustomer
{
    public int Id { get; init; }

    public EmailAddress Email { get; init; } = null!;

    public string DisplayName { get; init; } = string.Empty;

    public string Role { get; init; } = string.Empty;

    public ImageUrl Avatar { get; init; } = null!;
}

