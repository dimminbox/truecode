using TrueCode.Finance.Domain.CurrencyAggregate;

namespace TrueCode.Finance.Application.Tests;

public class CurrenciesFixture
{
    public const int TotalCount = 25;

    public IReadOnlyList<Currency> Currencies { get; }

    public CurrenciesFixture()
    {
        var currencies = new List<Currency>();
        for (var i = 0; i < TotalCount; i++)
        {
            var code = $"AA{(char)('A' + i)}";
            currencies.Add(CurrencyTestData.Restore(i + 1, code, $"Валюта {i + 1}", i + 1));
        }

        Currencies = currencies;
    }
}
