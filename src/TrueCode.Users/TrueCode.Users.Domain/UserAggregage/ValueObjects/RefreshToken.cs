namespace TrueCode.Users.Domain.UserAggregage.ValueObjects;

public sealed record RefreshToken(string Hash, DateTimeOffset ExpiresAt)
{
    public bool IsValid(DateTimeOffset now) => now < ExpiresAt;
}
