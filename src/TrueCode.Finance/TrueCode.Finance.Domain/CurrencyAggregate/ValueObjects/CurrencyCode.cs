using System.Text.RegularExpressions;
using TrueCode.Finance.Domain.CurrencyAggregate.Exceptions;

namespace TrueCode.Finance.Domain.CurrencyAggregate.ValueObjects;

public sealed partial record CurrencyCode
{
    public const int Length = 3;

    public string Value { get; }

    private CurrencyCode(string value) => Value = value;

    public static CurrencyCode Create(string? value)
    {
        var normalized = value?.Trim().ToUpperInvariant();

        if (string.IsNullOrEmpty(normalized))
            throw new InvalidCurrencyCodeException("Код валюты не может быть пустым.");
        if (!CodeRegex().IsMatch(normalized))
            throw new InvalidCurrencyCodeException($"Код валюты должен состоять из {Length} латинских букв.");

        return new CurrencyCode(normalized);
    }

    public override string ToString() => Value;

    [GeneratedRegex("^[A-Z]{3}$")]
    private static partial Regex CodeRegex();
}
