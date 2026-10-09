using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TrueCode.Users.Application.Auth;

namespace TrueCode.Users.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<AuthOptions>()
            .Bind(configuration.GetSection(AuthOptions.SectionName))
            .Validate(AuthOptions.IsValid, "Секция Auth: Issuer, Audience и TTL токенов обязательны.")
            .ValidateOnStart();

        services.AddMediatR(c => c.RegisterServicesFromAssemblyContaining<AuthOptions>());
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<TokenPairIssuer>();
        return services;
    }
}
