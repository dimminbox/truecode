using System.ComponentModel.DataAnnotations;

namespace TrueCode.Finance.Contracts.Currencies;

public sealed record CreateCurrencyRequest(
    [Required] string Code,
    [Required] string Name,
    decimal Rate);

public sealed record UpdateCurrencyRequest(decimal Rate);

public sealed record CreateCurrencyResponse(int Id);

public sealed record UpdateCurrencyResponse(int Id, string Code, string Name, decimal Rate, DateTimeOffset UpdatedAt);

public sealed record GetCurrencyResponse(int Id, string Code, string Name, decimal Rate, DateTimeOffset UpdatedAt);

public sealed record GetCurrenciesResponse(IReadOnlyList<GetCurrencyResponse> Items);
