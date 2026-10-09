using TrueCode.Users.Domain.Users;

namespace TrueCode.Users.Domain.UserAggregage.ValueObjects;

public sealed record RawPassword
{
    public const int MinLength = 8;
    public const int MaxLength = 128;

    public string Value { get; }

    private RawPassword(string value) => Value = value;

    public static RawPassword Create(string? value)
    {
        if (string.IsNullOrEmpty(value))
            throw new InvalidPasswordException("Пароль не может быть пустым.");
        if (value.Length < MinLength || value.Length > MaxLength)
            throw new InvalidPasswordException($"Длина пароля должна быть от {MinLength} до {MaxLength} символов.");
        if (!value.Any(char.IsLetter) || !value.Any(char.IsDigit))
            throw new InvalidPasswordException("Пароль должен содержать хотя бы одну букву и одну цифру.");

        return new RawPassword(value);
    }
}
