using MediatR;
using TrueCode.SharedKernel.Domain;
using TrueCode.SharedKernel.Results;
using TrueCode.Users.Application.Auth;
using TrueCode.Users.Application.Commands.Common;
using TrueCode.Users.Application.Errors;
using TrueCode.Users.Domain.AuthHistory;
using TrueCode.Users.Domain.UserAggregage;
using TrueCode.Users.Domain.UserAggregage.ValueObjects;
using TrueCode.Users.Domain.Users;

namespace TrueCode.Users.Application.Commands.LoginUser;

internal sealed class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, Result<LoginUserResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserAuthHistoryRepository _userAuthHistoryRepository;
    private readonly IPasswordHasher _hasher;
    private readonly TokenPairIssuer _tokens;
    private readonly TimeProvider _clock;

    public LoginUserCommandHandler(IUserRepository userRepository, IUserAuthHistoryRepository userAuthHistoryRepository, IPasswordHasher hasher, TokenPairIssuer tokens, TimeProvider clock)
    {
        _userRepository = userRepository;
        _userAuthHistoryRepository = userAuthHistoryRepository;
        _hasher = hasher;
        _tokens = tokens;
        _clock = clock;
    }

    public async Task<Result<LoginUserResponse>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        Login login;
        RawPassword password;
        try
        {
            login = Login.Create(request.Login);
            password = RawPassword.Create(request.Password);
        }
        catch (DomainException)
        {
            return UserErrors.InvalidCredentials<LoginUserResponse>();
        }

        var user = await _userRepository.GetByLoginAsync(login, cancellationToken);
        if (user == null)
            return UserErrors.InvalidCredentials<LoginUserResponse>();

        var now = _clock.GetUtcNow();

        if (!user.Authenticate(password, _hasher, now))
        {
            await _userAuthHistoryRepository.AddAsync(
                UserAuthHistoryEntry.Record(user.Id, AuthEventType.LoginFailed, now), cancellationToken);
            return UserErrors.InvalidCredentials<LoginUserResponse>();
        }

        var pair = await _tokens.IssueAsync(user, now, cancellationToken);
        await _userAuthHistoryRepository.AddAsync(
            UserAuthHistoryEntry.Record(user.Id, AuthEventType.LoginSucceeded, now), cancellationToken);

        return new Result<LoginUserResponse>
        {
            Data = new LoginUserResponse(pair.AccessToken, pair.RefreshToken, pair.ExpiresIn, pair.TokenType)
        };
    }
}
