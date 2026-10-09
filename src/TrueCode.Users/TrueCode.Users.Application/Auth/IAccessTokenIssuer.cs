using TrueCode.Users.Domain.UserAggregage;

namespace TrueCode.Users.Application.Auth;

public interface IAccessTokenIssuer
{
    IssuedAccessToken Issue(User user, DateTimeOffset now);

    JsonWebKeySet GetPublicKeys();
}

public sealed record IssuedAccessToken(string Token, int ExpiresIn);

public sealed record JsonWebKeySet(IReadOnlyList<JsonWebKey> Keys);

public sealed record JsonWebKey(string Kty, string Use, string Alg, string Kid, string N, string E);
