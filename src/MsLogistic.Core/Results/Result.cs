namespace MsLogistic.Core.Results;

public class Result {
    public bool IsSuccess { get; }
    public Error Error { get; }
    public bool IsFailure => !IsSuccess;

    protected Result(bool isSuccess, Error error) {
        if (isSuccess && error != Error.None || !isSuccess && error == Error.None) {
            throw new ArgumentException("Invalid error", nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new Result(true, Error.None);
    public static Result Failure(Error error) => new Result(false, error);
    public static Result<TValue> Success<TValue>(TValue value) => new Result<TValue>(value, true, Error.None);
    public static Result<TValue> Failure<TValue>(Error error) => new Result<TValue>(default, false, error);
}

public class Result<TValue> : Result {
    private readonly TValue? _value;

    protected internal Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error) {
        _value = value;
    }

    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failure result cannot be accessed.");

    public static implicit operator Result<TValue>(TValue value) =>
        Success(value);
}
