using MediatR;
using TrueCode.Finance.Application.Common;
using TrueCode.SharedKernel.Results;

namespace TrueCode.Finance.Application.Commands.DeleteCurrency;

public record DeleteCurrencyCommand(int Id) : IRequest<Result<DeleteCurrencyResponse>>;
