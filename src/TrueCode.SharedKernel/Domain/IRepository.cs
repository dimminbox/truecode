using System.Transactions;

namespace TrueCode.SharedKernel.Domain;

public interface IRepository<TEntity, TId>
    where TEntity : Entity<TId>
    where TId : notnull
{
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken ct = default);

    Task<TId> AddAsync(TEntity entity, CancellationToken ct = default);

    Task UpdateAsync(TEntity entity, CancellationToken ct = default);

    TransactionScope BeginTransaction();

    Task LockAsync(long key, CancellationToken ct);
}
