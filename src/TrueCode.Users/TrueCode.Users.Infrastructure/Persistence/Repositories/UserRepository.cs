using TrueCode.SharedKernel.Persistence;
using TrueCode.Users.Domain.UserAggregage;
using TrueCode.Users.Domain.UserAggregage.ValueObjects;
using TrueCode.Users.Domain.UserAggregate.ValueObjects;
using TrueCode.Users.Domain.Users;

namespace TrueCode.Users.Infrastructure.Persistence.Repositories;

internal sealed class UserRepository
    : Repository<User, Guid, UserRepository.UserRow>, IUserRepository
{
    public UserRepository(IDbConnectionFactory connectionFactory)
        : base(connectionFactory)
    {
    }

    protected override string TableName => "users";

    protected override string SelectColumns =>
        """
        id, name, login, password_hash AS PasswordHash, created_at AS CreatedAt, last_login_at AS LastLoginAt,
        refresh_token_hash AS RefreshTokenHash, refresh_token_expires_at AS RefreshTokenExpiresAt
        """;

    protected override string InsertSql =>
        """
        INSERT INTO users (id, name, login, password_hash, created_at, last_login_at,
                           refresh_token_hash, refresh_token_expires_at)
        VALUES (@Id, @Name, @Login, @PasswordHash, @CreatedAt, @LastLoginAt,
                @RefreshTokenHash, @RefreshTokenExpiresAt)
        """;

    protected override string UpdateSql =>
        """
        UPDATE users
        SET name = @Name, login = @Login, password_hash = @PasswordHash, last_login_at = @LastLoginAt,
            refresh_token_hash = @RefreshTokenHash, refresh_token_expires_at = @RefreshTokenExpiresAt
        WHERE id = @Id
        """;

    public Task<User?> GetByLoginAsync(Login login, CancellationToken ct = default) =>
        GetAsync("login = @login", new { login = login.Value }, ct);

    public Task<User?> GetByRefreshTokenHashAsync(string hash, CancellationToken ct = default) =>
        GetAsync("refresh_token_hash = @hash", new { hash }, ct);

    public async Task<bool> ExistsByLoginAsync(Login login, CancellationToken ct = default) =>
        await ExecuteScalarAsync<bool>(
            $"SELECT EXISTS (SELECT 1 FROM {TableName} WHERE login = @login)", new { login = login.Value }, ct);

    private async Task<User?> GetAsync(string where, object param, CancellationToken ct)
    {
        var row = await QuerySingleOrDefaultAsync<UserRow>(
            $"SELECT {SelectColumns} FROM {TableName} WHERE {where}", param, ct);
        return row == null ? null : ToDomainObject(row);
    }

    protected override User ToDomainObject(UserRow row) => User.Restore(
        row.Id, UserName.Create(row.Name), Login.Create(row.Login), new PasswordHash(row.PasswordHash),
        new DateTimeOffset(row.CreatedAt, TimeSpan.Zero),
        row.LastLoginAt == null ? null : new DateTimeOffset(row.LastLoginAt.Value, TimeSpan.Zero),
        row.RefreshTokenHash != null && row.RefreshTokenExpiresAt != null
            ? new RefreshToken(row.RefreshTokenHash, new DateTimeOffset(row.RefreshTokenExpiresAt.Value, TimeSpan.Zero))
            : null);

    protected override object ToObject(User user) => new
    {
        user.Id,
        Name = user.Name.Value,
        Login = user.Login.Value,
        PasswordHash = user.PasswordHash.Value,
        user.CreatedAt,
        user.LastLoginAt,
        RefreshTokenHash = user.RefreshToken?.Hash,
        RefreshTokenExpiresAt = user.RefreshToken?.ExpiresAt,
    };

    internal sealed record UserRow(
        Guid Id, string Name, string Login, string PasswordHash,
        DateTime CreatedAt, DateTime? LastLoginAt,
        string? RefreshTokenHash, DateTime? RefreshTokenExpiresAt);
}
