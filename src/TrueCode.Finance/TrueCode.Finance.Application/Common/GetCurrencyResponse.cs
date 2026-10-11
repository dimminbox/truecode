namespace TrueCode.Finance.Application.Common;

public record GetCurrencyResponse(string Code, string Name, decimal Rate, DateTimeOffset UpdatedAt);
