using MediatR;
using TrueCode.Finance.Application.Common;
using TrueCode.Finance.Domain.CurrencyAggregate;
using TrueCode.Finance.Domain.CurrencyAggregate.ValueObjects;
using TrueCode.SharedKernel.Domain;
using TrueCode.SharedKernel.Results;

namespace TrueCode.Finance.Application.Commands.ImportCurrencyRates;

internal sealed class ImportCurrencyRatesCommandHandler
    : IRequestHandler<ImportCurrencyRatesCommand, Result<ImportCurrencyRatesResponse>>
{
    private const long ImportLockKey = 15;

    private readonly ICurrencyRepository _currencyRepository;
    private readonly TimeProvider _clock;

    public ImportCurrencyRatesCommandHandler(ICurrencyRepository currencyRepository, TimeProvider clock)
    {
        _currencyRepository = currencyRepository;
        _clock = clock;
    }

    public async Task<Result<ImportCurrencyRatesResponse>> Handle(
        ImportCurrencyRatesCommand request, CancellationToken cancellationToken)
    {
        var result = new Result<ImportCurrencyRatesResponse>();

        using var transaction = _currencyRepository.BeginTransaction();
        await _currencyRepository.LockAsync(ImportLockKey, cancellationToken);

        var now = _clock.GetUtcNow();
        var existing = await _currencyRepository.GetAllAsync(ct: cancellationToken);
        var currenciesByCode = existing.ToDictionary(c => c.Code.Value);
        var usedNames = existing.Select(c => c.Name.Value).ToHashSet();
        var created = 0;
        var updated = 0;
        var unchanged = 0;

        foreach (var item in request.Rates)
        {
            CurrencyCode code;
            CurrencyName name;
            CurrencyRate rate;
            try
            {
                code = CurrencyCode.Create(item.Code);
                name = CurrencyName.Create(item.Name);
                rate = CurrencyRate.Create(item.Rate);
            }
            catch (DomainException ex)
            {
                result.AddError($"{item.Code}: {ex.Message}");
                continue;
            }

            if (!currenciesByCode.TryGetValue(code.Value, out var currency))
            {
                if (usedNames.Contains(name.Value))
                {
                    result.AddError($"{code.Value}: название '{name.Value}' уже занято другой валютой.");
                    continue;
                }

                var newCurrency = Currency.Create(code, name, rate, now);
                await _currencyRepository.AddAsync(newCurrency, cancellationToken);
                currenciesByCode[code.Value] = newCurrency;
                usedNames.Add(name.Value);
                created++;
            }
            else if (currency.Rate != rate)
            {
                currency.SetRate(currency, rate, now);
                await _currencyRepository.UpdateAsync(currency, cancellationToken);
                updated++;
            }
            else
            {
                unchanged++;
            }
        }

        transaction.Complete();

        result.Data = new ImportCurrencyRatesResponse(created, updated, unchanged);
        return result;
    }
}
