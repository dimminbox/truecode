namespace TrueCode.Finance.Application.Common;

public record UpdateCurrencyResponse(int Id, string Code, string Name, decimal Rate, DateTimeOffset UpdatedAt);
