using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace TrueCode.SharedKernel.Persistence;

public static class PersistenceExtensions
{
    public static IServiceCollection AddNpgsqlPersistence(this IServiceCollection services, string connectionString)
    {
        services.AddSingleton(NpgsqlDataSource.Create(connectionString));
        services.AddSingleton<IDbConnectionFactory, NpgsqlConnectionFactory>();
        return services;
    }
}
