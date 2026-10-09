using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using TrueCode.Users.Application.Auth;
using TrueCode.Users.Domain.AuthHistory;
using TrueCode.Users.Domain.Users;
using TrueCode.Users.Infrastructure.Auth;
using TrueCode.SharedKernel.Persistence;
using TrueCode.Users.Domain.UserAggregage;
using TrueCode.Users.Infrastructure.Persistence.Repositories;

namespace TrueCode.Users.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Не задана строка подключения ConnectionStrings:Postgres.");

        services.AddNpgsqlPersistence(connectionString);
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserAuthHistoryRepository, UserAuthHistoryRepository>();

        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<IAccessTokenIssuer, JwtAccessTokenIssuer>();
        return services;
    }
}
