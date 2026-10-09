using MediatR;
using TrueCode.Finance.Application.Common;
using TrueCode.Finance.Application.Errors;
using TrueCode.Finance.Domain.CurrencyAggregate;
using TrueCode.Finance.Domain.CurrencyAggregate.ValueObjects;
using TrueCode.SharedKernel.Domain;
using TrueCode.SharedKernel.Results;

namespace TrueCode.Finance.Application.Commands.CreateCurrency;

internal sealed class CreateCurrencyCommandHandler
    : IRequestHandler<CreateCurrencyCommand, Result<CreateCurrencyResponse>>
{
    private readonly ICurrencyRepository _currencyRepository;
    private readonly TimeProvider _clock;

    public CreateCurrencyCommandHandler(ICurrencyRepository currencyRepository, TimeProvider clock)
    {
        _currencyRepository = currencyRepository;
        _clock = clock;
    }

    public async Task<Result<CreateCurrencyResponse>> Handle(CreateCurrencyCommand request, CancellationToken cancellationToken)
    {
        CurrencyCode code;
        CurrencyName name;
        CurrencyRate rate;
        try
        {
            code = CurrencyCode.Create(request.Code);
            name = CurrencyName.Create(request.Name);
            rate = CurrencyRate.Create(request.Rate);
        }
        catch (DomainException ex)
        {
            return CurrencyErrors.Invalid<CreateCurrencyResponse>(ex.Message);
        }

        if (await _currencyRepository.ExistsByCodeAsync(code, cancellationToken))
            return CurrencyErrors.CodeTaken<CreateCurrencyResponse>(code.Value);

        if (await _currencyRepository.ExistsByNameAsync(name, cancellationToken))
            return CurrencyErrors.NameTaken<CreateCurrencyResponse>(name.Value);

        var currency = Currency.Create(code, name, rate, _clock.GetUtcNow());
        var id = await _currencyRepository.AddAsync(currency, cancellationToken);

        return new Result<CreateCurrencyResponse> { Data = new CreateCurrencyResponse(id.Value) };
    }
}
