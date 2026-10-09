using Microsoft.Extensions.DependencyInjection;

namespace TrueCode.Finance.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(c => c.RegisterServicesFromAssemblyContaining<CurrencyMarker>());
        services.AddSingleton(TimeProvider.System);
        return services;
    }

    private sealed class CurrencyMarker;
}
