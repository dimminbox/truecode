using System.ComponentModel.DataAnnotations;

namespace TrueCode.Users.Contracts.Auth;

public sealed record RegisterRequest(
    [Required] string Name,
    [Required] string Login,
    [Required] string Password);

public sealed record LoginRequest(
    [Required] string Login,
    [Required] string Password);

public sealed record RefreshTokenRequest(
    [Required] string RefreshToken);

public sealed record RegisterResponse(Guid Id);

public sealed record LoginResponse(string AccessToken, string RefreshToken, int ExpiresIn, string TokenType);

public sealed record RefreshTokenResponse(string AccessToken, string RefreshToken, int ExpiresIn, string TokenType);
