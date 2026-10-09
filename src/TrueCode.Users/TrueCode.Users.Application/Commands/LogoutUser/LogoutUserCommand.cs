using MediatR;
using TrueCode.SharedKernel.Results;

namespace TrueCode.Users.Application.Commands;

public record LogoutUserCommand(string RefreshToken) : IRequest<Result<LogoutUserResponse>>;
