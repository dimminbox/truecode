using MediatR;
using TrueCode.Finance.Application.Common;
using TrueCode.SharedKernel.Results;

namespace TrueCode.Finance.Application.Commands.UpdateCurrency;

public record UpdateCurrencyCommand(int Id, decimal Rate) : IRequest<Result<UpdateCurrencyResponse>>;
