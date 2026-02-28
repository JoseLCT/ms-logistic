using MsLogistic.Core.Results;

namespace MsLogistic.Domain.Shared.Errors;

public static class BoundariesErrors {
    public static Error InsufficientPoints(int minimumPoints) =>
        Error.Validation(
            code: "Boundaries.InsufficientPoints",
            message: $"A polygon must have at least {minimumPoints} points."
        );

    public static Error ConsecutiveDuplicatePoints =>
        Error.Validation(
            code: "Boundaries.ConsecutiveDuplicatePoints",
            message: "The boundaries contain consecutive duplicate points."
        );

    public static Error DuplicatePoints =>
        Error.Validation(
            code: "Boundaries.DuplicatePoints",
            message: "The boundaries contain duplicate points."
        );
}
