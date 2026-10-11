using System.Data.Common;
using System.Transactions;
using Dapper;
using TrueCode.SharedKernel.Domain;

namespace TrueCode.SharedKernel.Persistence;

public abstract class Repository<TEntity, TId, TRow> : IRepository<TEntity, TId>
    where TEntity : Entity<TId>
    where TId : notnull
{
    private readonly IDbConnectionFactory _connectionFactory;

    protected Repository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    protected abstract string TableName { get; }

    protected abstract string SelectColumns { get; }

    protected abstract string InsertSql { get; }

    protected virtual string UpdateSql => throw new NotSupportedException($"{GetType().Name} не поддерживает изменение.");

    protected virtual string IdColumn => "id";

    protected abstract TEntity ToDomainObject(TRow row);

    protected abstract object ToObject(TEntity entity);

    public virtual async Task<TEntity?> GetByIdAsync(TId id, CancellationToken ct = default)
    {
        var row = await QuerySingleOrDefaultAsync<TRow>(
            $"SELECT {SelectColumns} FROM {TableName} WHERE {IdColumn} = @id", new { id }, ct);
        return row == null ? null : ToDomainObject(row);
    }

    public virtual async Task<TId> AddAsync(TEntity entity, CancellationToken ct = default)
    {
        await ExecuteAsync(InsertSql, ToObject(entity), ct);
        return entity.Id;
    }

    public virtual async Task UpdateAsync(TEntity entity, CancellationToken ct = default) =>
        await ExecuteAsync(UpdateSql, ToObject(entity), ct);

    public TransactionScope BeginTransaction() =>
        new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

    public async Task LockAsync(long key, CancellationToken ct) =>
        await ExecuteAsync("SELECT pg_advisory_xact_lock(@key)", new { key }, ct);

    protected Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? param, CancellationToken ct) =>
        RunAsync(connection => connection.QuerySingleOrDefaultAsync<T>(
            new CommandDefinition(sql, param, cancellationToken: ct)), ct);

    protected Task<IReadOnlyList<T>> QueryAsync<T>(string sql, object? param, CancellationToken ct) =>
        RunAsync<IReadOnlyList<T>>(async connection =>
            (await connection.QueryAsync<T>(new CommandDefinition(sql, param, cancellationToken: ct))).AsList(), ct);

    protected Task<T?> ExecuteScalarAsync<T>(string sql, object? param, CancellationToken ct) =>
        RunAsync(connection => connection.ExecuteScalarAsync<T>(
            new CommandDefinition(sql, param, cancellationToken: ct)), ct);

    protected Task<int> ExecuteAsync(string sql, object? param, CancellationToken ct) =>
        RunAsync(connection => connection.ExecuteAsync(
            new CommandDefinition(sql, param, cancellationToken: ct)), ct);

    private async Task<T> RunAsync<T>(Func<DbConnection, Task<T>> action, CancellationToken ct)
    {
        await using var connection = await _connectionFactory.OpenAsync(ct);
        return await action(connection);
    }
}
