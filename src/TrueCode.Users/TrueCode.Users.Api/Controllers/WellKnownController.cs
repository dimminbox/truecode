using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TrueCode.Users.Contracts.Auth;
using TrueCode.Users.Application.Auth;

namespace TrueCode.Users.Api.Controllers;

[ApiController]
[Route(".well-known")]
public sealed class WellKnownController : ControllerBase
{
    private readonly IOptions<AuthOptions> _options;
    private readonly IAccessTokenIssuer _issuer;

    public WellKnownController(IOptions<AuthOptions> options, IAccessTokenIssuer issuer)
    {
        _options = options;
        _issuer = issuer;
    }

    [HttpGet("openid-configuration")]
    public DiscoveryDocument OpenIdConfiguration() => new(
        _options.Value.Issuer,
        $"{Request.Scheme}://{Request.Host}/.well-known/jwks.json",
        ["RS256"]);

    [HttpGet("jwks.json")]
    public JsonWebKeySet Jwks() => _issuer.GetPublicKeys();
}