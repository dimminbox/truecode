using TrueCode.SharedKernel.Domain;

namespace TrueCode.Users.Domain.AuthHistory;

/// <summary>Неизменяемая запись журнала входов пользователя (аудит, не состояние авторизации).</summary>
public sealed class UserAuthHistoryEntry : Entity<Guid>
{
    public Guid UserId { get; private init; }
    public AuthEventType EventType { get; private init; }
    public DateTimeOffset OccurredAt { get; private init; }

    public static UserAuthHistoryEntry Record(Guid userId, AuthEventType eventType, DateTimeOffset now) =>
        new() { Id = Guid.NewGuid(), UserId = userId, EventType = eventType, OccurredAt = now };

    public static UserAuthHistoryEntry Restore(Guid id, Guid userId, AuthEventType eventType, DateTimeOffset occurredAt) =>
        new() { Id = id, UserId = userId, EventType = eventType, OccurredAt = occurredAt };
}
