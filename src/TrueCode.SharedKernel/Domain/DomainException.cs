namespace TrueCode.SharedKernel.Domain;

public class DomainException : Exception
{
    public DomainException(string message)
        : base(message)
    {
    }
}
