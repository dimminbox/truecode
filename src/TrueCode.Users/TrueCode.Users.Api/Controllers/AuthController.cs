using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TrueCode.Users.Api.Mapping;
using TrueCode.Users.Api.Security;
using TrueCode.Users.Contracts.Auth;

namespace TrueCode.Users.Api.Controllers;

[ServiceFilter(typeof(ApiTokenFilter))]
[Route("auth")]
public sealed class AuthController : ApiControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken ct)
    {
        var result = await _sender.Send(request.ToCommand(), ct);

        return Match(result, response => Created($"/users/{response.Id}", response.ToResponse()));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken ct)
    {
        var result = await _sender.Send(request.ToCommand(), ct);

        return Match(result, response => Ok(response.ToResponse()), StatusCodes.Status401Unauthorized);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshTokenRequest request, CancellationToken ct)
    {
        var result = await _sender.Send(request.ToRefreshCommand(), ct);

        return Match(result, response => Ok(response.ToResponse()), StatusCodes.Status401Unauthorized);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(RefreshTokenRequest request, CancellationToken ct)
    {
        var result = await _sender.Send(request.ToLogoutCommand(), ct);

        return Match(result, _ => NoContent());
    }
}
