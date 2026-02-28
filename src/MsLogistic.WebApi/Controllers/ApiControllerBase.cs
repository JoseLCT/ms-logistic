using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MsLogistic.Core.Results;

namespace MsLogistic.WebApi.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase {
    protected IActionResult HandleResult<T>(Result<T> result) {
        return result.IsSuccess
            ? Ok(result.Value)
            : HandleFailure(result);
    }

    protected IActionResult HandleResult(Result result) {
        return result.IsSuccess
            ? Ok()
            : HandleFailure(result);
    }

    private IActionResult HandleFailure(Result result) {
        return result.Error.Type switch {
            ErrorType.NotFound => NotFound(CreateProblemDetails(
                "Resource Not Found",
                StatusCodes.Status404NotFound,
                result.Error)),

            ErrorType.Validation => BadRequest(CreateProblemDetails(
                "Validation Failed",
                StatusCodes.Status400BadRequest,
                result.Error)),

            ErrorType.Conflict => Conflict(CreateProblemDetails(
                "Conflict Occurred",
                StatusCodes.Status409Conflict,
                result.Error)),

            ErrorType.Unauthorized => Unauthorized(CreateProblemDetails(
                "Unauthorized Access",
                StatusCodes.Status401Unauthorized,
                result.Error)),

            ErrorType.Forbidden => StatusCode(
                StatusCodes.Status403Forbidden,
                CreateProblemDetails(
                    "Forbidden",
                    StatusCodes.Status403Forbidden,
                    result.Error)),

            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                CreateProblemDetails(
                    "Internal Server Error",
                    StatusCodes.Status500InternalServerError,
                    result.Error))
        };
    }


    private ProblemDetails CreateProblemDetails(
        string title,
        int status,
        Error error) {
        return new ProblemDetails {
            Title = title,
            Status = status,
            Detail = error.Message,
            Extensions =
            {
                ["errorCode"] = error.Code,
                ["timestamp"] = DateTime.UtcNow
            }
        };
    }
}
