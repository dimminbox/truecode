namespace TrueCode.Users.Application.Auth;

internal sealed record TokenPair(string AccessToken, string RefreshToken, int ExpiresIn, string TokenType = "Bearer");
