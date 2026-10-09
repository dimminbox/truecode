namespace TrueCode.Users.Application.Auth;

/// <summary>Секция <c>Auth</c> в конфигурации.</summary>
public sealed class AuthOptions
{
    public const string SectionName = "Auth";

    public string Issuer { get; init; } = "";
    public string Audience { get; init; } = "";
    public TimeSpan AccessTokenLifetime { get; init; }
    public TimeSpan RefreshTokenLifetime { get; init; }

    /// <summary>Приватный RSA-ключ в PEM. Если не задан — генерируется временный (токены не переживут рестарт).</summary>
    public string? SigningKeyPem { get; init; }

    internal static bool IsValid(AuthOptions o) =>
        !string.IsNullOrWhiteSpace(o.Issuer)
        && !string.IsNullOrWhiteSpace(o.Audience)
        && o.AccessTokenLifetime > TimeSpan.Zero
        && o.RefreshTokenLifetime > TimeSpan.Zero;
}
