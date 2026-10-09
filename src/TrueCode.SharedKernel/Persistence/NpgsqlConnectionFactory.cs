using System.Data.Common;
using Npgsql;

namespace TrueCode.SharedKernel.Persistence;

public sealed class NpgsqlConnectionFactory : IDbConnectionFactory
{
    private readonly NpgsqlDataSource _dataSource;

    public NpgsqlConnectionFactory(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<DbConnection> OpenAsync(CancellationToken ct) =>
        await _dataSource.OpenConnectionAsync(ct);
}
