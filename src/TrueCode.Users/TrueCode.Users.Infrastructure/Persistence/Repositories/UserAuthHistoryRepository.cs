using TrueCode.SharedKernel.Persistence;
using TrueCode.Users.Domain.AuthHistory;

namespace TrueCode.Users.Infrastructure.Persistence.Repositories;

internal sealed class UserAuthHistoryRepository
    : Repository<UserAuthHistoryEntry, Guid, UserAuthHistoryRepository.EntryRow>, IUserAuthHistoryRepository
{
    public UserAuthHistoryRepository(IDbConnectionFactory connectionFactory)
        : base(connectionFactory)
    {
    }

    protected override string TableName => "user_auth_history";

    protected override string SelectColumns =>
        "id, user_id AS UserId, event_type AS EventType, occurred_at AS OccurredAt";

    protected override string InsertSql =>
        """
        INSERT INTO user_auth_history (id, user_id, event_type, occurred_at)
        VALUES (@Id, @UserId, @EventType, @OccurredAt)
        """;

    public async Task<IReadOnlyList<UserAuthHistoryEntry>> GetByUserAsync(
        Guid userId, int limit, CancellationToken ct = default)
    {
        var rows = await QueryAsync<EntryRow>(
            $"SELECT {SelectColumns} FROM {TableName} WHERE user_id = @userId ORDER BY occurred_at DESC LIMIT @limit",
            new { userId, limit }, ct);
        return rows.Select(ToDomainObject).ToList();
    }

    protected override UserAuthHistoryEntry ToDomainObject(EntryRow row) =>
        UserAuthHistoryEntry.Restore(row.Id, row.UserId, Enum.Parse<AuthEventType>(row.EventType),
            new DateTimeOffset(row.OccurredAt, TimeSpan.Zero));

    protected override object ToObject(UserAuthHistoryEntry entry) => new
    {
        entry.Id,
        entry.UserId,
        EventType = entry.EventType.ToString(),
        entry.OccurredAt,
    };

    internal sealed record EntryRow(Guid Id, Guid UserId, string EventType, DateTime OccurredAt);
}
