using TrueCode.Users.Domain.UserAggregage;

namespace TrueCode.Users.Domain.Users;

/// <summary>Готовый хеш пароля (результат <see cref="IPasswordHasher"/>), не сам пароль.</summary>
public sealed record PasswordHash
{
    public string Value { get; }

    public PasswordHash(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Хеш пароля не может быть пустым.", nameof(value));
        Value = value;
    }

    public override string ToString() => Value;
}
