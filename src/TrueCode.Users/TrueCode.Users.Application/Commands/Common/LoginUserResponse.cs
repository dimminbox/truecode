namespace TrueCode.Users.Application.Commands.Common;

public record LoginUserResponse(string AccessToken, string RefreshToken, int ExpiresIn, string TokenType);
