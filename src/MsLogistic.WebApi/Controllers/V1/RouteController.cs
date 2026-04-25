using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MsLogistic.Application.Routes.GetAllRoutes;
using MsLogistic.Application.Routes.GetRouteById;
using MsLogistic.Application.Routes.StartRoute;
using MsLogistic.Core.Results;

namespace MsLogistic.WebApi.Controllers.V1;

[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/routes")]
public class RouteController : ApiControllerBase {
	private readonly IMediator _mediator;

	public RouteController(IMediator mediator) {
		_mediator = mediator;
	}

	[HttpGet]
	[ProducesResponseType(typeof(Result<IReadOnlyList<RouteSummaryDto>>), StatusCodes.Status200OK)]
	public async Task<IActionResult> GetAll() {
		var query = new GetAllRoutesQuery();
		Result<IReadOnlyList<RouteSummaryDto>> result = await _mediator.Send(query);
		return HandleResult(result);
	}

	[HttpGet("{id:guid}")]
	[ProducesResponseType(typeof(Result<RouteDetailDto>), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> GetById(Guid id) {
		var query = new GetRouteByIdQuery(id);
		Result<RouteDetailDto> result = await _mediator.Send(query);
		return HandleResult(result);
	}

	[HttpPost("{id:guid}")]
	public async Task<IActionResult> StartRoute(Guid id) {
		var command = new StartRouteCommand(id);
		Result result = await _mediator.Send(command);
		return HandleResult(result);
	}
}
