using TrueCode.Finance.Application.Commands.CreateCurrency;
using TrueCode.Finance.Application.Commands.UpdateCurrency;
using TrueCode.Finance.Application.Common;
using TrueCode.Finance.Contracts.Currencies;

namespace TrueCode.Finance.Api.Mapping;

internal static class CurrencyMappings
{
    public static CreateCurrencyCommand ToCommand(this CreateCurrencyRequest request) =>
        new(request.Code, request.Name, request.Rate);

    public static UpdateCurrencyCommand ToCommand(this UpdateCurrencyRequest request, int id) =>
        new(id, request.Rate);

    public static Contracts.Currencies.CreateCurrencyResponse ToResponse(this Application.Common.CreateCurrencyResponse response) =>
        new(response.Id);

    public static Contracts.Currencies.UpdateCurrencyResponse ToResponse(this Application.Common.UpdateCurrencyResponse response) =>
        new(response.Id, response.Code, response.Name, response.Rate, response.UpdatedAt);

    public static Contracts.Currencies.GetCurrencyResponse ToResponse(this Application.Common.GetCurrencyResponse response) =>
        new(response.Id, response.Code, response.Name, response.Rate, response.UpdatedAt);

    public static Contracts.Currencies.GetCurrenciesResponse ToResponse(this Application.Common.GetCurrenciesResponse response) =>
        new(response.Items.Select(item => item.ToResponse()).ToList());
}
