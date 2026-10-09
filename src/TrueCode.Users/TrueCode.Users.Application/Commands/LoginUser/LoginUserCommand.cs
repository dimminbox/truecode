using MediatR;
using TrueCode.SharedKernel.Results;
using TrueCode.Users.Application.Commands.Common;

namespace TrueCode.Users.Application.Commands.LoginUser;

public record LoginUserCommand(string Login, string Password) : IRequest<Result<LoginUserResponse>>;
