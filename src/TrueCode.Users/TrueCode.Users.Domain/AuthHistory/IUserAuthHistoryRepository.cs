namespace TrueCode.Users.Domain.AuthHistory;

public interface IUserAuthHistoryRepository
{
    Task<Guid> AddAsync(UserAuthHistoryEntry entry, CancellationToken ct = default);

    Task<IReadOnlyList<UserAuthHistoryEntry>> GetByUserAsync(Guid userId, int limit, CancellationToken ct = default);
}
