using TrueCode.Users.Domain.Users;

namespace TrueCode.Users.Domain.UserAggregate.ValueObjects;

public sealed record UserName
{
    public const int MaxLength = 100;

    public string Value { get; }

    private UserName(string value) => Value = value;

    public static UserName Create(string? value)
    {
        var trimmed = value?.Trim();

        if (string.IsNullOrEmpty(trimmed))
            throw new InvalidUserNameException("Имя не может быть пустым.");
        if (trimmed.Length > MaxLength)
            throw new InvalidUserNameException($"Имя не может быть длиннее {MaxLength} символов.");

        return new UserName(trimmed);
    }

    public override string ToString() => Value;
}