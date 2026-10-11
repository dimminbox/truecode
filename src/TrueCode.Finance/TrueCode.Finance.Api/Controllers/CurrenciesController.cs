using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrueCode.Finance.Api.Mapping;
using TrueCode.Finance.Application.Queries.GetCurrencies;
using TrueCode.Finance.Application.Queries.GetCurrency;

namespace TrueCode.Finance.Api.Controllers;

[Authorize]
[Route("currencies")]
public sealed class CurrenciesController : ApiControllerBase
{
    private readonly ISender _sender;

    public CurrenciesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("{code}")]
    public async Task<IActionResult> Get(string code, CancellationToken ct)
    {
        var result = await _sender.Send(new GetCurrencyQuery(code), ct);

        return Match(result, response => Ok(response.ToResponse()), StatusCodes.Status404NotFound);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _sender.Send(new GetCurrenciesQuery(page, pageSize), ct);

        return Match(result, response => Ok(response.ToResponse()));
    }
}
