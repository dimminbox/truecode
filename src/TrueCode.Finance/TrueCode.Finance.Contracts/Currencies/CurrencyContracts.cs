namespace TrueCode.Finance.Contracts.Currencies;

public sealed record GetCurrencyResponse(string Code, string Name, decimal Rate, DateTimeOffset UpdatedAt);
