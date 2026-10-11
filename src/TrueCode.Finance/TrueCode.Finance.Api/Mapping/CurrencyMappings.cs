namespace TrueCode.Finance.Api.Mapping;

internal static class CurrencyMappings
{
    public static Contracts.Currencies.GetCurrencyResponse ToResponse(this Application.Common.GetCurrencyResponse response) =>
        new(response.Code, response.Name, response.Rate, response.UpdatedAt);

    public static IReadOnlyList<Contracts.Currencies.GetCurrencyResponse> ToResponse(
        this IReadOnlyList<Application.Common.GetCurrencyResponse> response) =>
        response.Select(item => item.ToResponse()).ToList();
}
