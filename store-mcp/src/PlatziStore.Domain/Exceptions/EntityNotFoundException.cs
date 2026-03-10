using System;

namespace PlatziStore.Domain.Exceptions;

public sealed class EntityNotFoundException : Exception
{
    public string EntityType { get; }

    public object EntityId { get; }

    public EntityNotFoundException(string entityType, object entityId)
        : base($"Entity '{entityType}' with id '{entityId}' was not found.")
    {
        EntityType = entityType ?? throw new ArgumentNullException(nameof(entityType));
        EntityId = entityId ?? throw new ArgumentNullException(nameof(entityId));
    }
}

