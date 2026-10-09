using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using TrueCode.Finance.Domain.CurrencyAggregate;
using TrueCode.Finance.Infrastructure.Persistence;
using TrueCode.Finance.Infrastructure.Persistence.Repositories;
using TrueCode.SharedKernel.Persistence;

namespace TrueCode.Finance.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
                               ?? throw new InvalidOperationException(
                                   "Не задана строка подключения ConnectionStrings:Postgres.");

        SqlMapper.AddTypeHandler(new CurrencyIdTypeHandler());

        services.AddNpgsqlPersistence(connectionString);
        services.AddScoped<ICurrencyRepository, CurrencyRepository>();
        return services;
    }
}