using MediatR;
using TrueCode.SharedKernel.Results;

namespace TrueCode.Users.Application.Commands;

public record RegisterUserCommand(string Name, string Login, string Password) : IRequest<Result<RegisterUserResponse>>;
