using TrueCode.SharedKernel.Domain;
using TrueCode.Users.Domain.UserAggregage;

namespace TrueCode.Users.Domain.Users;

public interface IUserRepository : IRepository<User, Guid>
{
    Task<User?> GetByLoginAsync(Login login, CancellationToken ct = default);

    Task<User?> GetByRefreshTokenHashAsync(string hash, CancellationToken ct = default);

    Task<bool> ExistsByLoginAsync(Login login, CancellationToken ct = default);
}
