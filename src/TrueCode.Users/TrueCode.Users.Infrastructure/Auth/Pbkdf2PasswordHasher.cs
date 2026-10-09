using System.Security.Cryptography;
using TrueCode.Users.Domain.UserAggregage;
using TrueCode.Users.Domain.UserAggregage.ValueObjects;
using TrueCode.Users.Domain.Users;

namespace TrueCode.Users.Infrastructure.Auth;

/// <summary>PBKDF2-SHA256 из BCL, без внешних зависимостей. Формат хеша: <c>iterations.salt.hash</c> (base64).</summary>
internal sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const int Iterations = 600_000;
    private const int SaltSize = 16;
    private const int HashSize = 32;

    public PasswordHash Hash(RawPassword password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Derive(password.Value, salt, Iterations, HashSize);
        return new PasswordHash($"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}");
    }

    public bool Verify(RawPassword password, PasswordHash hash)
    {
        var parts = hash.Value.Split('.');
        if (parts.Length != 3 || !int.TryParse(parts[0], out var iterations))
            return false;

        try
        {
            var expected = Convert.FromBase64String(parts[2]);
            var actual = Derive(password.Value, Convert.FromBase64String(parts[1]), iterations, expected.Length);
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static byte[] Derive(string password, byte[] salt, int iterations, int size) =>
        Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, size);
}
