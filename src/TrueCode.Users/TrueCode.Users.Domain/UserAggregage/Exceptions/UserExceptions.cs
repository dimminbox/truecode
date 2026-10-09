using TrueCode.SharedKernel.Domain;

namespace TrueCode.Users.Domain.Users;

public sealed class InvalidLoginException : DomainException
{
    public InvalidLoginException(string message)
        : base(message)
    {
    }
}

public sealed class InvalidUserNameException : DomainException
{
    public InvalidUserNameException(string message)
        : base(message)
    {
    }
}

public sealed class InvalidPasswordException : DomainException
{
    public InvalidPasswordException(string message)
        : base(message)
    {
    }
}
