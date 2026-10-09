using MediatR;
using TrueCode.Finance.Application.Common;
using TrueCode.Finance.Application.Errors;
using TrueCode.Finance.Domain.CurrencyAggregate;
using TrueCode.Finance.Domain.CurrencyAggregate.ValueObjects;
using TrueCode.SharedKernel.Domain;
using TrueCode.SharedKernel.Results;

namespace TrueCode.Finance.Application.Queries.GetCurrency;

internal sealed class GetCurrencyQueryHandler
    : IRequestHandler<GetCurrencyQuery, Result<GetCurrencyResponse>>
{
    private readonly ICurrencyRepository _currencyRepository;

    public GetCurrencyQueryHandler(ICurrencyRepository currencyRepository)
    {
        _currencyRepository = currencyRepository;
    }

    public async Task<Result<GetCurrencyResponse>> Handle(GetCurrencyQuery request, CancellationToken cancellationToken)
    {
        CurrencyId id;
        try
        {
            id = CurrencyId.Create(request.Id);
        }
        catch (DomainException ex)
        {
            return CurrencyErrors.Invalid<GetCurrencyResponse>(ex.Message);
        }

        var currency = await _currencyRepository.GetByIdAsync(id, cancellationToken);
        if (currency == null)
            return CurrencyErrors.NotFound<GetCurrencyResponse>(request.Id);

        return new Result<GetCurrencyResponse>
        {
            Data = new GetCurrencyResponse(currency.Id.Value, currency.Code.Value, currency.Name.Value, currency.Rate.Value, currency.UpdatedAt)
        };
    }
}
