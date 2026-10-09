using MediatR;
using TrueCode.SharedKernel.Results;

namespace TrueCode.Users.Application.Commands;

public record RefreshTokenCommand(string RefreshToken) : IRequest<Result<RefreshTokenResponse>>;
