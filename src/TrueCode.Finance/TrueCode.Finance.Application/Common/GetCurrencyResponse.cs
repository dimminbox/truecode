namespace TrueCode.Finance.Application.Common;

public record GetCurrencyResponse(int Id, string Code, string Name, decimal Rate, DateTimeOffset UpdatedAt);
