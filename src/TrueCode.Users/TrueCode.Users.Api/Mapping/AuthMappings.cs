using TrueCode.Users.Application.Commands;
using TrueCode.Users.Application.Commands.Common;
using TrueCode.Users.Application.Commands.LoginUser;
using TrueCode.Users.Contracts.Auth;

namespace TrueCode.Users.Api.Mapping;

internal static class AuthMappings
{
    public static RegisterUserCommand ToCommand(this RegisterRequest request) =>
        new(request.Name, request.Login, request.Password);

    public static LoginUserCommand ToCommand(this LoginRequest request) =>
        new(request.Login, request.Password);

    public static RefreshTokenCommand ToRefreshCommand(this RefreshTokenRequest request) =>
        new(request.RefreshToken);

    public static LogoutUserCommand ToLogoutCommand(this RefreshTokenRequest request) =>
        new(request.RefreshToken);

    public static RegisterResponse ToResponse(this RegisterUserResponse response) =>
        new(response.Id);

    public static LoginResponse ToResponse(this LoginUserResponse response) =>
        new(response.AccessToken, response.RefreshToken, response.ExpiresIn, response.TokenType);

    public static Contracts.Auth.RefreshTokenResponse ToResponse(this Application.Commands.RefreshTokenResponse response) =>
        new(response.AccessToken, response.RefreshToken, response.ExpiresIn, response.TokenType);
}
