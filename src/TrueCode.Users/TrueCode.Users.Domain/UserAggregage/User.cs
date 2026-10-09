using TrueCode.SharedKernel.Domain;
using TrueCode.Users.Domain.UserAggregage.ValueObjects;
using TrueCode.Users.Domain.UserAggregate.ValueObjects;
using TrueCode.Users.Domain.Users;

namespace TrueCode.Users.Domain.UserAggregage;

public sealed class User : AggregateRoot<Guid>
{
    public UserName Name { get; private set; }
    public Login Login { get; private set; }
    public PasswordHash PasswordHash { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? LastLoginAt { get; private set; }
    public RefreshToken? RefreshToken { get; private set; }

    private User(Guid id, UserName name, Login login, PasswordHash passwordHash, DateTimeOffset createdAt)
    {
        Id = id;
        Name = name;
        Login = login;
        PasswordHash = passwordHash;
        CreatedAt = createdAt;
    }

    public static User Register(
        UserName name, Login login, RawPassword password, IPasswordHasher hasher, DateTimeOffset now)
    {
        var user = new User(Guid.NewGuid(), name, login, hasher.Hash(password), now);
        return user;
    }

    /// <summary>Восстановление агрегата из хранилища (без бизнес-проверок и событий).</summary>
    public static User Restore(
        Guid id, UserName name, Login login, PasswordHash passwordHash,
        DateTimeOffset createdAt, DateTimeOffset? lastLoginAt, RefreshToken? refreshToken) =>
        new(id, name, login, passwordHash, createdAt) { LastLoginAt = lastLoginAt, RefreshToken = refreshToken };

    public bool Authenticate(RawPassword password, IPasswordHasher hasher, DateTimeOffset now)
    {
        if (!hasher.Verify(password, PasswordHash))
            return false;

        LastLoginAt = now;
        return true;
    }
    
    public void IssueRefreshToken(string hash, DateTimeOffset now, TimeSpan lifetime)
    {
        if (lifetime <= TimeSpan.Zero)
            throw new DomainException("Время жизни refresh-токена должно быть положительным.");

        RefreshToken = new RefreshToken(hash, now + lifetime);
    }

    public bool CanRefresh(DateTimeOffset now) => RefreshToken?.IsValid(now) == true;
    
    public void RevokeRefreshToken() => RefreshToken = null;
}
