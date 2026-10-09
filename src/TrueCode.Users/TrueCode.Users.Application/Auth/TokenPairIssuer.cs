using Microsoft.Extensions.Options;
using TrueCode.Users.Domain.UserAggregage;
using TrueCode.Users.Domain.Users;

namespace TrueCode.Users.Application.Auth;

internal sealed class TokenPairIssuer
{
    private readonly IUserRepository _userRepository;
    private readonly IAccessTokenIssuer _accessTokens;
    private readonly IOptions<AuthOptions> _options;

    public TokenPairIssuer(IUserRepository userRepository, IAccessTokenIssuer accessTokens, IOptions<AuthOptions> options)
    {
        _userRepository = userRepository;
        _accessTokens = accessTokens;
        _options = options;
    }

    public async Task<TokenPair> IssueAsync(User user, DateTimeOffset now, CancellationToken ct)
    {
        var (raw, hash) = RefreshTokens.Generate();
        user.IssueRefreshToken(hash, now, _options.Value.RefreshTokenLifetime);
        await _userRepository.UpdateAsync(user, ct);

        var access = _accessTokens.Issue(user, now);
        return new TokenPair(access.Token, raw, access.ExpiresIn);
    }
}
