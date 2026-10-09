namespace TrueCode.Finance.Application.Common;

public record GetCurrenciesResponse(IReadOnlyList<GetCurrencyResponse> Items);
