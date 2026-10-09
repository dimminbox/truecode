using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using TrueCode.Users.Application.Auth;
using TrueCode.Users.Domain.UserAggregage;
using JsonWebKey = TrueCode.Users.Application.Auth.JsonWebKey;
using JsonWebKeySet = TrueCode.Users.Application.Auth.JsonWebKeySet;

namespace TrueCode.Users.Infrastructure.Auth;

internal sealed class JwtAccessTokenIssuer : IAccessTokenIssuer
{
    private readonly AuthOptions _options;
    private readonly RsaSecurityKey _key;
    private readonly JsonWebKey _publicKey;
    private readonly JsonWebTokenHandler _handler = new();

    public JwtAccessTokenIssuer(IOptions<AuthOptions> options, ILogger<JwtAccessTokenIssuer> logger)
    {
        _options = options.Value;

        var rsa = RSA.Create(2048);
        if (string.IsNullOrWhiteSpace(_options.SigningKeyPem))
            logger.LogWarning("Auth:SigningKeyPem не задан — сгенерирован временный ключ, токены перестанут действовать после рестарта.");
        else
            rsa.ImportFromPem(_options.SigningKeyPem);

        var parameters = rsa.ExportParameters(includePrivateParameters: false);
        var kid = Base64UrlEncoder.Encode(SHA256.HashData([.. parameters.Modulus!, .. parameters.Exponent!]))[..16];

        _key = new RsaSecurityKey(rsa) { KeyId = kid };
        _publicKey = new JsonWebKey(
            "RSA", "sig", SecurityAlgorithms.RsaSha256, kid,
            Base64UrlEncoder.Encode(parameters.Modulus), Base64UrlEncoder.Encode(parameters.Exponent));
    }

    public IssuedAccessToken Issue(User user, DateTimeOffset now)
    {
        var token = _handler.CreateToken(new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            IssuedAt = now.UtcDateTime,
            NotBefore = now.UtcDateTime,
            Expires = (now + _options.AccessTokenLifetime).UtcDateTime,
            Subject = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("login", user.Login.Value),
                new Claim("name", user.Name.Value),
            ]),
            SigningCredentials = new SigningCredentials(_key, SecurityAlgorithms.RsaSha256),
        });

        return new IssuedAccessToken(token, (int)_options.AccessTokenLifetime.TotalSeconds);
    }

    public JsonWebKeySet GetPublicKeys() => new([_publicKey]);
}
