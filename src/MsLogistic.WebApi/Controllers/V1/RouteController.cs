using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MsLogistic.Application.Routes.GetAllRoutes;

namespace MsLogistic.WebApi.Controllers.V1;

[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/routes")]
public class RouteController : ApiControllerBase {
    private readonly IMediator _mediator;

    public RouteController(IMediator mediator) {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() {
        var query = new GetAllRoutesQuery();
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }
}
