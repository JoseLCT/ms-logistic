using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MsLogistic.Application.Drivers.CreateDriver;
using MsLogistic.Application.Drivers.GetAllDrivers;
using MsLogistic.Application.Drivers.GetDriverById;
using MsLogistic.Application.Drivers.RemoveDriver;
using MsLogistic.Application.Drivers.UpdateDriver;
using MsLogistic.Core.Results;
using MsLogistic.WebApi.Contracts.V1.Drivers;

namespace MsLogistic.WebApi.Controllers.V1;

[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/drivers")]
public class DriverController : ApiControllerBase {
	private readonly IMediator _mediator;

	public DriverController(IMediator mediator) {
		_mediator = mediator;
	}

	[HttpGet]
	[ProducesResponseType(typeof(Result<IReadOnlyList<DriverSummaryDto>>), StatusCodes.Status200OK)]
	public async Task<IActionResult> GetAll() {
		var query = new GetAllDriversQuery();
		Result<IReadOnlyList<DriverSummaryDto>> result = await _mediator.Send(query);
		return HandleResult(result);
	}

	[HttpGet("{id:guid}")]
	[ProducesResponseType(typeof(Result<DriverDetailDto>), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(Result<DriverDetailDto>), StatusCodes.Status404NotFound)]
	public async Task<IActionResult> GetById(Guid id) {
		var query = new GetDriverByIdQuery(id);
		Result<DriverDetailDto> result = await _mediator.Send(query);
		return HandleResult(result);
	}

	[HttpPost]
	[ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status201Created)]
	public async Task<IActionResult> Create([FromBody] CreateDriverContract contract) {
		var command = new CreateDriverCommand(contract.FullName);
		Result<Guid> result = await _mediator.Send(command);
		return HandleCreatedResult(
			result,
			nameof(GetById),
			new { id = result.IsSuccess ? result.Value : Guid.Empty }
		);
	}

	[HttpPut("{id:guid}")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
	public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDriverContract contract) {
		var command = new UpdateDriverCommand(id, contract.FullName, contract.IsActive);
		Result result = await _mediator.Send(command);
		return HandleNoContentResult(result);
	}

	[HttpDelete("{id:guid}")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
	public async Task<IActionResult> Remove(Guid id) {
		var command = new RemoveDriverCommand(id);
		Result result = await _mediator.Send(command);
		return HandleNoContentResult(result);
	}
}
