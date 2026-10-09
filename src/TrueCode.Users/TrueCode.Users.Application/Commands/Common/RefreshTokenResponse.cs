namespace TrueCode.Users.Application.Commands;

public record RefreshTokenResponse(string AccessToken, string RefreshToken, int ExpiresIn, string TokenType);
