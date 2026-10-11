using TrueCode.Finance.Domain.CurrencyAggregate;
using TrueCode.Finance.Domain.CurrencyAggregate.ValueObjects;

namespace TrueCode.Finance.Application.Tests;

internal static class CurrencyTestData
{
    public static readonly DateTimeOffset Now = DateTimeOffset.Parse("2026-10-11T12:00:00Z");

    public static Currency Restore(int id, string code, string name, decimal rate, DateTimeOffset? updatedAt = null) =>
        Currency.Restore(
            CurrencyId.Create(id),
            CurrencyCode.Create(code),
            CurrencyName.Create(name),
            CurrencyRate.Create(rate),
            updatedAt ?? Now.AddDays(-1));
}
