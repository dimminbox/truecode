using MediatR;
using TrueCode.Finance.Application.Common;
using TrueCode.Finance.Application.Errors;
using TrueCode.Finance.Domain.CurrencyAggregate;
using TrueCode.Finance.Domain.CurrencyAggregate.ValueObjects;
using TrueCode.SharedKernel.Domain;
using TrueCode.SharedKernel.Results;

namespace TrueCode.Finance.Application.Commands.UpdateCurrency;

internal sealed class UpdateCurrencyCommandHandler
    : IRequestHandler<UpdateCurrencyCommand, Result<UpdateCurrencyResponse>>
{
    private readonly ICurrencyRepository _currencyRepository;
    private readonly TimeProvider _clock;

    public UpdateCurrencyCommandHandler(ICurrencyRepository currencyRepository, TimeProvider clock)
    {
        _currencyRepository = currencyRepository;
        _clock = clock;
    }

    public async Task<Result<UpdateCurrencyResponse>> Handle(UpdateCurrencyCommand request, CancellationToken cancellationToken)
    {
        CurrencyId id;
        CurrencyRate rate;
        try
        {
            id = CurrencyId.Create(request.Id);
            rate = CurrencyRate.Create(request.Rate);
        }
        catch (DomainException ex)
        {
            return CurrencyErrors.Invalid<UpdateCurrencyResponse>(ex.Message);
        }

        var currency = await _currencyRepository.GetByIdAsync(id, cancellationToken);
        if (currency == null)
            return CurrencyErrors.NotFound<UpdateCurrencyResponse>(request.Id);

        currency.SetRate(currency, rate, _clock.GetUtcNow());
        await _currencyRepository.UpdateAsync(currency, cancellationToken);

        return new Result<UpdateCurrencyResponse>
        {
            Data = new UpdateCurrencyResponse(currency.Id.Value, currency.Code.Value, currency.Name.Value, currency.Rate.Value, currency.UpdatedAt)
        };
    }
}
