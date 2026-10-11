using TrueCode.Users.Domain.UserAggregage;
using TrueCode.Users.Domain.UserAggregage.ValueObjects;
using TrueCode.Users.Domain.UserAggregate.ValueObjects;
using TrueCode.Users.Domain.Users;

namespace TrueCode.Users.Application.Tests;

internal static class UserTestData
{
    public const string LoginValue = "user1";
    public const string PasswordValue = "password1";
    public const string PasswordHashValue = "password-hash";

    public static readonly DateTimeOffset Now = DateTimeOffset.Parse("2026-10-11T12:00:00Z");

    public static User RestoreUser(RefreshToken? refreshToken = null) =>
        User.Restore(
            Guid.NewGuid(),
            UserName.Create("Иван"),
            Login.Create(LoginValue),
            new PasswordHash(PasswordHashValue),
            Now.AddDays(-1),
            null,
            refreshToken);
}
