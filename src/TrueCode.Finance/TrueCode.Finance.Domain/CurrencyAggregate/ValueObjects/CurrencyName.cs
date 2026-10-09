using TrueCode.Finance.Domain.CurrencyAggregate.Exceptions;

namespace TrueCode.Finance.Domain.CurrencyAggregate.ValueObjects;

public sealed record CurrencyName
{
    public const int MaxLength = 100;

    public string Value { get; }

    private CurrencyName(string value) => Value = value;

    public static CurrencyName Create(string? value)
    {
        var trimmed = value?.Trim();

        if (string.IsNullOrEmpty(trimmed))
            throw new InvalidCurrencyNameException("Название валюты не может быть пустым.");
        if (trimmed.Length > MaxLength)
            throw new InvalidCurrencyNameException($"Название валюты не может быть длиннее {MaxLength} символов.");

        return new CurrencyName(trimmed);
    }

    public override string ToString() => Value;
}
