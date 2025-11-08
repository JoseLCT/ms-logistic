using Microsoft.AspNetCore.Mvc;
using MsLogistic.Core.Results;

namespace MsLogistic.WebApi.Controllers;

[ApiController]
public abstract class BaseController : ControllerBase
{
    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return result.Error.Type switch
        {
            ErrorType.NotFound => NotFound(new
            {
                error = result.Error.Code,
                message = result.Error.StructuredMessage
            }),
            ErrorType.Validation => BadRequest(new
            {
                error = result.Error.Code,
                message = result.Error.StructuredMessage
            }),
            ErrorType.Conflict => Conflict(new
            {
                error = result.Error.Code,
                message = result.Error.StructuredMessage
            }),
            _ => StatusCode(500, new
            {
                error = result.Error.Code,
                message = result.Error.StructuredMessage
            })
        };
    }
}