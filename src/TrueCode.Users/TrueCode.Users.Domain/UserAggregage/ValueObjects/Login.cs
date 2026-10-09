using System.Text.RegularExpressions;
using TrueCode.SharedKernel.Domain;

namespace TrueCode.Users.Domain.Users;

public sealed record Login
{
    public const int MinLength = 3;
    public const int MaxLength = 50;

    public string Value { get; }

    private Login(string value) => Value = value;

    public static Login Create(string? value)
    {
        var normalized = value?.Trim().ToLowerInvariant();

        if (string.IsNullOrEmpty(normalized))
            throw new InvalidLoginException("Логин не может быть пустым.");
        if (normalized.Length < MinLength || normalized.Length > MaxLength)
            throw new InvalidLoginException($"Длина логина должна быть от {MinLength} до {MaxLength} символов.");
        if (!AllowedChars(normalized))
            throw new InvalidLoginException("Логин может содержать только латинские буквы, цифры, '.', '_' и '-'.");

        return new Login(normalized);
    }

    public override string ToString() => Value;

    private static readonly Regex AllowedCharsRegex = new("^[a-z0-9._-]+$", RegexOptions.Compiled);

    private static bool AllowedChars(string value) => AllowedCharsRegex.IsMatch(value);
}
