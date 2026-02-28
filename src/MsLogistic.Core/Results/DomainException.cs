namespace MsLogistic.Core.Results;

public class DomainException : Exception {
    public Error Error { get; }

    public DomainException(Error error)
        : base(error.Message) {
        Error = error;
    }

    public DomainException(Error error, Exception innerException)
        : base(error.Message, innerException) {
        Error = error;
    }
}
