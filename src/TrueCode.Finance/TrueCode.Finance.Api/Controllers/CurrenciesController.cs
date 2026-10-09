using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrueCode.Finance.Api.Mapping;
using TrueCode.Finance.Application.Commands.DeleteCurrency;
using TrueCode.Finance.Application.Queries.GetCurrencies;
using TrueCode.Finance.Application.Queries.GetCurrency;
using TrueCode.Finance.Contracts.Currencies;

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

    [HttpPost]
    public async Task<IActionResult> Create(CreateCurrencyRequest request, CancellationToken ct)
    {
        var result = await _sender.Send(request.ToCommand(), ct);

        return Match(result, response => Created($"/currencies/{response.Id}", response.ToResponse()));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateCurrencyRequest request, CancellationToken ct)
    {
        var result = await _sender.Send(request.ToCommand(id), ct);

        return Match(result, response => Ok(response.ToResponse()));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken ct)
    {
        var result = await _sender.Send(new GetCurrencyQuery(id), ct);

        return Match(result, response => Ok(response.ToResponse()), StatusCodes.Status404NotFound);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _sender.Send(new GetCurrenciesQuery(page, pageSize), ct);

        return Match(result, response => Ok(response.ToResponse()));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await _sender.Send(new DeleteCurrencyCommand(id), ct);

        return Match(result, _ => NoContent(), StatusCodes.Status404NotFound);
    }
}
