using MediatR;
using TrueCode.Finance.Application.Common;
using TrueCode.SharedKernel.Results;

namespace TrueCode.Finance.Application.Queries.GetCurrencies;

public record GetCurrenciesQuery(int Page, int PageSize) : IRequest<Result<GetCurrenciesResponse>>;
