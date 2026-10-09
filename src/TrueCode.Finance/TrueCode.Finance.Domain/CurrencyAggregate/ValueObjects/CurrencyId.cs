using TrueCode.Finance.Domain.CurrencyAggregate.Exceptions;

namespace TrueCode.Finance.Domain.CurrencyAggregate.ValueObjects;

public readonly record struct CurrencyId
{
    public int Value { get; }

    private CurrencyId(int value) => Value = value;

    public static CurrencyId Create(int value)
    {
        if (value <= 0)
            throw new InvalidCurrencyIdException("Идентификатор валюты должен быть положительным числом.");

        return new CurrencyId(value);
    }

    public override string ToString() => Value.ToString();
}
