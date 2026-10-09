using MediatR;
using TrueCode.SharedKernel.Results;
using TrueCode.Users.Application.Auth;
using TrueCode.Users.Application.Errors;
using TrueCode.Users.Domain.AuthHistory;
using TrueCode.Users.Domain.Users;

namespace TrueCode.Users.Application.Commands.RefreshToken;

internal sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<RefreshTokenResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserAuthHistoryRepository _userAuthHistoryRepository;
    private readonly TokenPairIssuer _tokens;
    private readonly TimeProvider _clock;

    public RefreshTokenCommandHandler(IUserRepository userRepository, IUserAuthHistoryRepository userAuthHistoryRepository, TokenPairIssuer tokens, TimeProvider clock)
    {
        _userRepository = userRepository;
        _userAuthHistoryRepository = userAuthHistoryRepository;
        _tokens = tokens;
        _clock = clock;
    }

    public async Task<Result<RefreshTokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return UserErrors.InvalidRefreshToken<RefreshTokenResponse>();

        var user = await _userRepository.GetByRefreshTokenHashAsync(RefreshTokens.Hash(request.RefreshToken), cancellationToken);
        var now = _clock.GetUtcNow();
        if (user == null || !user.CanRefresh(now))
            return UserErrors.InvalidRefreshToken<RefreshTokenResponse>();

        var pair = await _tokens.IssueAsync(user, now, cancellationToken);
        await _userAuthHistoryRepository.AddAsync(
            UserAuthHistoryEntry.Record(user.Id, AuthEventType.TokenRefreshed, now), cancellationToken);

        return new Result<RefreshTokenResponse>
        {
            Data = new RefreshTokenResponse(pair.AccessToken, pair.RefreshToken, pair.ExpiresIn, pair.TokenType)
        };
    }
}
