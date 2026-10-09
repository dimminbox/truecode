using TrueCode.Finance.Domain.CurrencyAggregate.ValueObjects;
using TrueCode.SharedKernel.Domain;

namespace TrueCode.Finance.Domain.CurrencyAggregate;

public sealed class Currency : AggregateRoot<CurrencyId>
{
    public CurrencyCode Code { get; private set; }
    public CurrencyName Name { get; private set; }
    public CurrencyRate Rate { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private Currency(CurrencyCode code, CurrencyName name, CurrencyRate rate, DateTimeOffset updatedAt)
    {
        Code = code;
        Name = name;
        Rate = rate;
        UpdatedAt = updatedAt;
    }

    private Currency(CurrencyId id, CurrencyCode code, CurrencyName name, CurrencyRate rate, DateTimeOffset updatedAt)
        : this(code, name, rate, updatedAt)
    {
        Id = id;
    }

    public static Currency Create(CurrencyCode code, CurrencyName name, CurrencyRate rate, DateTimeOffset now) =>
        new(code, name, rate, now);

    public static Currency Restore(
        CurrencyId id, CurrencyCode code, CurrencyName name, CurrencyRate rate, DateTimeOffset updatedAt) =>
        new(id, code, name, rate, updatedAt);

    public void SetRate(Currency currency, CurrencyRate rate, DateTimeOffset now)
    {
        currency.Rate = rate;
        currency.UpdatedAt = now;
    }
}