using MediatR;
using TrueCode.Finance.Application.Common;
using TrueCode.SharedKernel.Results;

namespace TrueCode.Finance.Application.Commands.CreateCurrency;

public record CreateCurrencyCommand(string Code, string Name, decimal Rate) : IRequest<Result<CreateCurrencyResponse>>;
