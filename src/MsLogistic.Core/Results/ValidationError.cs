namespace MsLogistic.Core.Results;

public sealed record ValidationError : Error {
    public ValidationFailure[] Failures { get; }

    private ValidationError(ValidationFailure[] failures)
        : base(
            "Validation.General",
            "One or more validation errors occurred",
            ErrorType.Validation) {
        Failures = failures;
    }

    public static ValidationError Create(params ValidationFailure[] failures) => new(failures);

    public static ValidationError FromErrors(params Error[] errors) {
        var failures = errors
            .Select(e => new ValidationFailure(e.Code, e.Message))
            .ToArray();

        return new ValidationError(failures);
    }
}

public sealed record ValidationFailure(string PropertyName, string ErrorMessage);
