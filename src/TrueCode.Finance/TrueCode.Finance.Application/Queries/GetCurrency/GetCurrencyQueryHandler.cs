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
        CurrencyCode code;
        try
        {
            code = CurrencyCode.Create(request.Code);
        }
        catch (DomainException ex)
        {
            return CurrencyErrors.Invalid<GetCurrencyResponse>(ex.Message);
        }

        var currency = await _currencyRepository.GetByCodeAsync(code, cancellationToken);
        if (currency == null)
            return CurrencyErrors.NotFound<GetCurrencyResponse>(code.Value);

        return new Result<GetCurrencyResponse>
        {
            Data = new GetCurrencyResponse(currency.Code.Value, currency.Name.Value, currency.Rate.Value, currency.UpdatedAt)
        };
    }
}
