using TrueCode.Users.Domain.UserAggregage.ValueObjects;
using TrueCode.Users.Domain.Users;

namespace TrueCode.Users.Domain.UserAggregage;

public interface IPasswordHasher
{
    PasswordHash Hash(RawPassword password);

    bool Verify(RawPassword password, PasswordHash hash);
}
