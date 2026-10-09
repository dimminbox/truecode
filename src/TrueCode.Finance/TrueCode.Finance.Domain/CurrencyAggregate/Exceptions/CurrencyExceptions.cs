using TrueCode.SharedKernel.Domain;

namespace TrueCode.Finance.Domain.CurrencyAggregate.Exceptions;

public sealed class InvalidCurrencyNameException : DomainException
{
    public InvalidCurrencyNameException(string message)
        : base(message)
    {
    }
}

public sealed class InvalidCurrencyRateException : DomainException
{
    public InvalidCurrencyRateException(string message)
        : base(message)
    {
    }
}

public sealed class InvalidCurrencyCodeException : DomainException
{
    public InvalidCurrencyCodeException(string message)
        : base(message)
    {
    }
}

public sealed class InvalidCurrencyIdException : DomainException
{
    public InvalidCurrencyIdException(string message)
        : base(message)
    {
    }
}
