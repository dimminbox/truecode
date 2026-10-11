using MediatR;
using TrueCode.Finance.Application.Common;
using TrueCode.SharedKernel.Results;

namespace TrueCode.Finance.Application.Queries.GetCurrency;

public record GetCurrencyQuery(string Code) : IRequest<Result<GetCurrencyResponse>>;
