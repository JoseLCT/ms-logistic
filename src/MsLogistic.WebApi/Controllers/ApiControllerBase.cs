using Microsoft.AspNetCore.Mvc;
using MsLogistic.Core.Results;

namespace MsLogistic.WebApi.Controllers;

[ApiController]
[ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
public abstract class ApiControllerBase : ControllerBase {
	protected IActionResult HandleResult<T>(Result<T> result) {
		return result.IsSuccess
			? Ok(result)
			: HandleFailure(result);
	}

	protected IActionResult HandleResult(Result result) {
		return result.IsSuccess
			? Ok(result)
			: HandleFailure(result);
	}

	protected IActionResult HandleCreatedResult<T>(
		Result<T> result,
		string actionName,
		object? routeValues = null
	) {
		return result.IsSuccess
			? CreatedAtAction(actionName, routeValues, result)
			: HandleFailure(result);
	}

	protected IActionResult HandleNoContentResult(Result result) {
		return result.IsSuccess
			// ? NoContent()
			? Ok(result)
			: HandleFailure(result);
	}

	private IActionResult HandleFailure(Result result) {
		int statusCode = result.Error?.Type switch {
			ErrorType.NotFound => StatusCodes.Status404NotFound,
			ErrorType.Validation => StatusCodes.Status400BadRequest,
			ErrorType.Conflict => StatusCodes.Status409Conflict,
			ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
			ErrorType.Forbidden => StatusCodes.Status403Forbidden,
			_ => StatusCodes.Status500InternalServerError
		};

		return StatusCode(statusCode, result);
	}
}
