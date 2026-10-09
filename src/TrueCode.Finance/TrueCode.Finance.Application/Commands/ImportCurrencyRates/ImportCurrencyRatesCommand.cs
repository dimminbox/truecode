using MediatR;
using TrueCode.Finance.Application.Common;
using TrueCode.SharedKernel.Results;

namespace TrueCode.Finance.Application.Commands.ImportCurrencyRates;

public record ImportCurrencyRateItem(string Code, string Name, decimal Rate);

public record ImportCurrencyRatesCommand(IReadOnlyList<ImportCurrencyRateItem> Rates)
    : IRequest<Result<ImportCurrencyRatesResponse>>;
