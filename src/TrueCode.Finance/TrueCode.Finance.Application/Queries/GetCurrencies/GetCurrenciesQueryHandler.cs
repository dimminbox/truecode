using MediatR;
using TrueCode.Finance.Application.Common;
using TrueCode.Finance.Application.Errors;
using TrueCode.Finance.Domain.CurrencyAggregate;
using TrueCode.SharedKernel.Results;

namespace TrueCode.Finance.Application.Queries.GetCurrencies;

internal sealed class GetCurrenciesQueryHandler
    : IRequestHandler<GetCurrenciesQuery, Result<IReadOnlyList<GetCurrencyResponse>>>
{
    private const int MaxPageSize = 100;

    private readonly ICurrencyRepository _currencyRepository;

    public GetCurrenciesQueryHandler(ICurrencyRepository currencyRepository)
    {
        _currencyRepository = currencyRepository;
    }

    public async Task<Result<IReadOnlyList<GetCurrencyResponse>>> Handle(GetCurrenciesQuery request, CancellationToken cancellationToken)
    {
        if (request.Page < 1)
            return CurrencyErrors.Invalid<IReadOnlyList<GetCurrencyResponse>>("Номер страницы должен быть не меньше 1.");
        if (request.PageSize < 1 || request.PageSize > MaxPageSize)
            return CurrencyErrors.Invalid<IReadOnlyList<GetCurrencyResponse>>($"Размер страницы должен быть от 1 до {MaxPageSize}.");

        var currencies = await _currencyRepository.GetAllAsync(
            (request.Page - 1) * request.PageSize, request.PageSize, cancellationToken);

        var items = currencies
            .Select(c => new GetCurrencyResponse(c.Code.Value, c.Name.Value, c.Rate.Value, c.UpdatedAt))
            .ToList();

        return new Result<IReadOnlyList<GetCurrencyResponse>> { Data = items };
    }
}
