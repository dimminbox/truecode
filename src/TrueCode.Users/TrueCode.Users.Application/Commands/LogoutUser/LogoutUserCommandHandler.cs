using MediatR;
using TrueCode.SharedKernel.Results;
using TrueCode.Users.Application.Auth;
using TrueCode.Users.Domain.AuthHistory;
using TrueCode.Users.Domain.Users;

namespace TrueCode.Users.Application.Commands;

internal sealed class LogoutUserCommandHandler : IRequestHandler<LogoutUserCommand, Result<LogoutUserResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserAuthHistoryRepository _userAuthHistoryRepository;
    private readonly TimeProvider _clock;

    public LogoutUserCommandHandler(IUserRepository userRepository, IUserAuthHistoryRepository userAuthHistoryRepository, TimeProvider clock)
    {
        _userRepository = userRepository;
        _userAuthHistoryRepository = userAuthHistoryRepository;
        _clock = clock;
    }

    public async Task<Result<LogoutUserResponse>> Handle(LogoutUserCommand request, CancellationToken cancellationToken)
    {
        var result = new Result<LogoutUserResponse> { Data = new LogoutUserResponse() };

        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return result;

        var user = await _userRepository.GetByRefreshTokenHashAsync(RefreshTokens.Hash(request.RefreshToken), cancellationToken);
        if (user == null)
            return result;

        user.RevokeRefreshToken();
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _userAuthHistoryRepository.AddAsync(
            UserAuthHistoryEntry.Record(user.Id, AuthEventType.LoggedOut, _clock.GetUtcNow()), cancellationToken);
        return result;
    }
}
