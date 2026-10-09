using MediatR;
using TrueCode.SharedKernel.Domain;
using TrueCode.SharedKernel.Results;
using TrueCode.Users.Application.Errors;
using TrueCode.Users.Domain.UserAggregage;
using TrueCode.Users.Domain.UserAggregage.ValueObjects;
using TrueCode.Users.Domain.UserAggregate.ValueObjects;
using TrueCode.Users.Domain.Users;

namespace TrueCode.Users.Application.Commands.RegisterUser;

internal sealed class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<RegisterUserResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _hasher;
    private readonly TimeProvider _clock;

    public RegisterUserCommandHandler(IUserRepository userRepository, IPasswordHasher hasher, TimeProvider clock)
    {
        _userRepository = userRepository;
        _hasher = hasher;
        _clock = clock;
    }

    public async Task<Result<RegisterUserResponse>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        UserName name;
        Login login;
        RawPassword password;
        try
        {
            name = UserName.Create(request.Name);
            login = Login.Create(request.Login);
            password = RawPassword.Create(request.Password);
        }
        catch (DomainException ex)
        {
            return UserErrors.Invalid<RegisterUserResponse>(ex.Message);
        }

        if (await _userRepository.ExistsByLoginAsync(login, cancellationToken))
            return UserErrors.LoginTaken<RegisterUserResponse>(login.Value);

        var user = User.Register(name, login, password, _hasher, _clock.GetUtcNow());
        await _userRepository.AddAsync(user, cancellationToken);
        return new Result<RegisterUserResponse> { Data = new RegisterUserResponse(user.Id) };
    }
}
