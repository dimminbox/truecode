using System.Data.Common;

namespace TrueCode.SharedKernel.Persistence;

public interface IDbConnectionFactory
{
    Task<DbConnection> OpenAsync(CancellationToken ct);
}
