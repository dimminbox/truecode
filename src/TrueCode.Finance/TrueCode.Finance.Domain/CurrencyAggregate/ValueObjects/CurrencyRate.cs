using TrueCode.Finance.Domain.CurrencyAggregate.Exceptions;

namespace TrueCode.Finance.Domain.CurrencyAggregate.ValueObjects;

public sealed record CurrencyRate
{
    public const int Scale = 6;
    public const decimal MaxValue = 999_999_999_999m;

    public decimal Value { get; }

    private CurrencyRate(decimal value) => Value = value;

    public static CurrencyRate Create(decimal value)
    {
        if (value <= 0)
            throw new InvalidCurrencyRateException("Курс валюты должен быть больше нуля.");
        if (value > MaxValue)
            throw new InvalidCurrencyRateException($"Курс валюты не может быть больше {MaxValue}.");

        return new CurrencyRate(Math.Round(value, Scale));
    }

    public override string ToString() => Value.ToString();
}
