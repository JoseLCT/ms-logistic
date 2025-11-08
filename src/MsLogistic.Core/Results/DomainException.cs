namespace MsLogistic.Core.Results;

public class DomainException(Error error) : Exception
{
    public Error Error { get; } = error;
}