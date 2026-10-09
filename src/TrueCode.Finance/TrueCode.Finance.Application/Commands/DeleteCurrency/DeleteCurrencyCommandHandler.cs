using MediatR;
using TrueCode.Finance.Application.Common;
using TrueCode.Finance.Application.Errors;
using TrueCode.Finance.Domain.CurrencyAggregate;
using TrueCode.Finance.Domain.CurrencyAggregate.ValueObjects;
using TrueCode.SharedKernel.Domain;
using TrueCode.SharedKernel.Results;

namespace TrueCode.Finance.Application.Commands.DeleteCurrency;

internal sealed class DeleteCurrencyCommandHandler
    : IRequestHandler<DeleteCurrencyCommand, Result<DeleteCurrencyResponse>>
{
    private readonly ICurrencyRepository _currencyRepository;

    public DeleteCurrencyCommandHandler(ICurrencyRepository currencyRepository)
    {
        _currencyRepository = currencyRepository;
    }

    public async Task<Result<DeleteCurrencyResponse>> Handle(DeleteCurrencyCommand request, CancellationToken cancellationToken)
    {
        CurrencyId id;
        try
        {
            id = CurrencyId.Create(request.Id);
        }
        catch (DomainException ex)
        {
            return CurrencyErrors.Invalid<DeleteCurrencyResponse>(ex.Message);
        }

        var currency = await _currencyRepository.GetByIdAsync(id, cancellationToken);
        if (currency == null)
            return CurrencyErrors.NotFound<DeleteCurrencyResponse>(request.Id);

        await _currencyRepository.DeleteAsync(currency, cancellationToken);

        return new Result<DeleteCurrencyResponse> { Data = new DeleteCurrencyResponse() };
    }
}
